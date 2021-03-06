classdef ChartPlot < gui.layout.ContentControlPanel & stream.Stream
    %CHARTPLOT Chart plot
    %   Class to plot timeseries chart style plot.  Conforms to Matlab System Object
    %   design.
    
    properties
        NumChannels=0;
        SamplesPerPage=1000;
        SampleRate=1;
        YLim;
        ChannelLabels = [];
        ChannelColors = [];
    end
    
    properties (SetAccess=private)
        HandleAxes;
        HandleLines = [];
        Index = 1;
        HandleCursor;
        DrawingAxes = false;
    end
    
    properties (Access=private)
        scalefactor;
    end
    
    properties (Dependent=true)
        SecondsPerPage;
    end
    
    methods
        
        function h = ChartPlot(varargin)
            
            p = inputParser;
            
            p.addParamValue('Parent',[],@ishandle);
            p.addParamValue('NumChannels',0,@isscalar);
            p.addParamValue('YLim',[-1 1],@(x)isvector(x)&&(length(x)==2));
            p.addParamValue('Title','ChartPlot',@ischar);
            p.addParamValue('SampleRate',1,@isscalar);
            p.addParamValue('SamplesPerPage',1000,@isscalar);
            
            p.StructExpand = true;
            
            p.parse(varargin{:});
            
            if isempty(p.Results.Parent)
                parent = figure('menubar','none','toolbar','none', ...
                    'color','k','name',p.Results.Title,'position',[0 0 1120 630]);
                movegui(parent,'center');
            else
                parent = p.Results.Parent;
            end
            
            h = h@gui.layout.ContentControlPanel('parent',parent);
            
            h.YLim = p.Results.YLim;
            h.SampleRate = p.Results.SampleRate;
            h.SamplesPerPage = p.Results.SamplesPerPage;
            
            h.NumChannels = p.Results.NumChannels;
            
            
            
            uicontrol('Style', 'pushbutton', 'String', 'Scale Up',...
                'parent',h.HandleControlPanel, 'Position', [10 5 70 30], ...
                'Callback', @(varargin)ScaleUp);
            
            uicontrol('Style', 'pushbutton', 'String', 'Scale Down',...
                'parent',h.HandleControlPanel, 'Position', [90 5 70 30], ...
                'Callback', @(varargin)ScaleDown);
            
            uicontrol('Style', 'text',...
                'parent',h.HandleControlPanel,'String', 'Secs/Page',...
                'Position', [170 5 70 30]);
            
            hp = uicontrol('Style', 'edit',...
                'parent',h.HandleControlPanel,'String','', ...
                'Position', [250 5 70 30],'Callback', @(varargin)SecPerPage);
            
            function ScaleUp
                h.YLim = h.YLim .* 0.9;
                set(h.HandleCursor,'xdata',[1 1] .* h.Index,'ydata',h.YLim);
            end
            
            function ScaleDown
                h.YLim = h.YLim .* 1.1;
                set(h.HandleCursor,'xdata',[1 1] .* h.Index,'ydata',h.YLim);
            end
            
            function SecPerPage
                h.SecondsPerPage = str2double(get(hp,'String'));
            end
            
        end
        
        function step(h,d)
            
            h.islocked = true;
            
            assert(size(d,2)==h.NumChannels,'ChartPlot:DataSizeError', ...
                'Incorrect data block size');
            
            ci = h.Index;
            idx = ci:ci+size(d,1)-1;
            
            data = get(h.HandleLines,'ydata');
            
            gt = idx>h.SamplesPerPage;
            
            idx = [idx(~gt) idx(gt)-h.SamplesPerPage];
            
            data = cat(1,data{:});
            
            if any(idx==1), h.scalefactor = mean(d,1); end
            
            try
                d = d - repmat(h.scalefactor,[size(d,1) 1]);
                data(:,idx) = d';
                set(h.HandleLines,{'ydata'},num2cell(data,2));
            catch e %#ok<NASGU>
                return;
            end
            
            h.Index = idx(end)+1;
            
            set(h.HandleCursor,'xdata',([1 1] .* h.Index) ./ h.SampleRate, ...
                'ydata',h.YLim);
            
            
            
        end
        
        function set.NumChannels(h,n)
            h.NumChannels = n;
            drawaxes(h);
        end
        
        function set.ChannelLabels(h,n)
            h.ChannelLabels = n;
            drawaxes(h);
        end
        
        function set.ChannelColors(h,n)
            h.ChannelColors = n;
            drawaxes(h);
        end
        
        function set.SamplesPerPage(h,n)
            h.SamplesPerPage = n;
            drawaxes(h);
        end
        
        function set.SampleRate(h,n)
            h.SampleRate = n;
            drawaxes(h);
        end
        
        function s = get.SecondsPerPage(h)
            s = h.SamplesPerPage./h.SampleRate;
        end
        
        function set.SecondsPerPage(h,n)
            h.SamplesPerPage = n * h.SampleRate;
        end
        
        function set.YLim(h,n)
            h.YLim = n;
            set(h.HandleAxes,'YLim',n); %#ok<MCSUP>
        end
        
        function n = get.YLim(h)
            n = h.YLim;
        end
        
        %         function close(h)
        %             delete(h);
        %         end
        
        function delete(h)
            if ishandle(h.HandleAxes), delete(h.HandleAxes); end
            delete@gui.layout.ContentControlPanel(h);
            delete@stream.Stream(h);
        end
        
        function reset(h)
            drawaxes(h);
        end
        
    end
    
    methods (Access=private)
        
        function drawaxes(h)
            
            h.DrawingAxes = true;
            
            delete(h.HandleAxes);
            h.HandleAxes = [];
            h.HandleLines = [];
            h.HandleCursor = [];
            h.Index = 1;
            
            n = h.NumChannels;
            
            width = 0.95;
            height = 0.94/n;
            x = 0.025;
            
            for i = 1:n
                y = ((n-i)*height) + 0.05;
                
                h.HandleAxes(i) = axes('Parent',h.HandleContentPanel, ...
                    'Position',[x y width height],'YLim',h.YLim);
                
                if (i==n), xlabel('Time (s)','parent',h.HandleAxes(i)); end
                
                if ~isempty(h.ChannelLabels)
                    ylabel(h.ChannelLabels{i},'fontsize',8,'parent',h.HandleAxes(i));
                end
                
                h.HandleLines(i) = line((1:h.SamplesPerPage)./h.SampleRate, ...
                    zeros(h.SamplesPerPage,1),'parent',h.HandleAxes(i), ...
                    'clipping','off','color','y');
                
                if ~isempty(h.ChannelColors)
                    set(h.HandleLines(i),'Color',h.ChannelColors{i});
                end
                
                h.HandleCursor(i) = line([0 0],h.YLim,'color','r', ...
                    'parent',h.HandleAxes(i));
            end
            
            
            
            set(h.HandleAxes(1:end-1),'XTickLabel',[]);
            
            set(h.HandleAxes,'color',[0 0 0],'box','on',...
                'ygrid','off','ycolor',[0.4 0.4 0.4], ...
                'xgrid','on','xcolor',[0.4 0.4 0.4], ...
                'TickLength',[0 0],'xlim',[0 h.SamplesPerPage/h.SampleRate], ...
                'YTickLabel',[],'YTick',[]);
            
            h.DrawingAxes = false;
            
        end
        
    end
    
end

