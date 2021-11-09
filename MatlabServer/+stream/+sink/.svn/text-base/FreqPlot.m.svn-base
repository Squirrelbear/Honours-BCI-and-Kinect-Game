classdef FreqPlot < gui.layout.ContentControlPanel & stream.StreamObject
    %FREQPLOT Frequency plot
    %   Class to plot frequency spectrum.  Conforms to Matlab System Object
    %   design.
    
    properties
    end
    
    properties (SetAccess=private)
        HandleAxes;
        HandleLines = [];
    end
    
    properties (Dependent=true)
        Scale
        FreqRange
    end
    
    methods
        
        function h = FreqPlot(varargin)
            
            p = inputParser;
            
            p.addParamValue('Parent',[],@ishandle);
            p.addParamValue('FreqRange',[1 100],@(x)isvector(x)&&(length(x)==2));
            p.addParamValue('Title','FreqPlot',@ischar);
            
            p.StructExpand = true;
            
            p.parse(varargin{:});
            
            if isempty(p.Results.Parent)
                parent = figure('menubar','none','toolbar','none', ...
                    'color','k','name',p.Results.Title);
                movegui(parent,'center');
            else
                parent = p.Results.Parent;
            end
            
            h = h@gui.layout.ContentControlPanel('parent',parent);
            
            h.HandleAxes = axes('Parent',h.HandleContentPanel);
            
            h.Scale = 'log';
            
            set(h.HandleAxes,'XLim',p.Results.FreqRange, ...
                'xgrid','on','ygrid','on','color','none','box','on',...
                'ycolor',[0.4 0.4 0.4],'xcolor',[0.4 0.4 0.4]);
            xlabel('Freq (Hz)','parent',h.HandleAxes);
            
            hb = uicontrol('Style', 'pushbutton', 'String', 'Linear',...
                'parent',h.HandleControlPanel, 'Position', [10 5 70 30], ...
                'Callback', @togglescale);
            
            function togglescale(varargin)
                if strcmp(h.Scale,'log')
                   h.Scale='linear'; set(hb,'String','Log'); 
                else
                   h.Scale='log'; set(hb,'String','Linear');  
                end
            end
            
            
        end
        
        function step(h,d,f)
            h.islocked = true;
            if isempty(h.HandleLines)
                h.HandleLines = line(f,d,'parent',h.HandleAxes);
            else
                for i = 1:length(h.HandleLines)
                    set(h.HandleLines(i),'ydata',d(:,i));
                end
            end
        end
        
        function close(h)
            delete(h);
        end
        
        function delete(h)
            if ishandle(h.HandleAxes), delete(h.HandleAxes); end
            delete@gui.layout.ContentControlPanel(h);
        end
        
        function set.Scale(h,s)
            set(h.HandleAxes,'YScale',s);
        end
        
        function s = get.Scale(h)
            s = get(h.HandleAxes,'YScale');
        end
        
        function s = get.FreqRange(h)
            s = get(h.HandleAxes,'Xlim');
        end
        
        function set.FreqRange(h,s)
            set(h.HandleAxes,'Xlim',s);
        end
        
    end
    
end

