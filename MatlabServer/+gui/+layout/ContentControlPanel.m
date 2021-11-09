classdef ContentControlPanel < handle
    %UNTITLED Summary of this class goes here
    %   Detailed explanation goes here
    
    properties
        
    end
    
    properties (SetAccess=protected)
        HandleSelf;
        HandleControlPanel;
        HandleContentPanel;
        ControlHeight = 40;
    end
    
    methods
        
        function h = ContentControlPanel(varargin)
            
            p = inputParser; 
            p.addParamValue('Parent',[],@ishandle);    
            p.StructExpand = true;  
            p.parse(varargin{:});

            if isempty(p.Results.Parent)
                parent = figure;
            else
                parent = p.Results.Parent;
            end
            
            h.HandleSelf = uipanel('BorderType','none','parent',parent, ...
                'resizefcn',@(varargin)resize(h));
          
            p = getpixelposition(h.HandleSelf);
            
            h.HandleControlPanel = uipanel('units','pixel', ...
                'position',[1 1 p(3) h.ControlHeight], ...
                'parent',h.HandleSelf, ...
                'BorderType','etchedin', ...
                'units','normalized');
            
            h.HandleContentPanel = uipanel('units','pixel', ...
                'position',[1 h.ControlHeight p(3) p(4)-h.ControlHeight], ...
                'parent',h.HandleSelf, ...
                'BackgroundColor',[0.1 0.1 0.1], ...
                'BorderType','none', ...
                'units','normalized');
        end
        
        
        function delete(h)
            if ishandle(h.HandleSelf), delete(h.HandleSelf); end
            if ishandle(h.HandleControlPanel), delete(h.HandleControlPanel); end
            if ishandle(h.HandleContentPanel), delete(h.HandleContentPanel); end
            clear h;
        end
        
        function close(h)
            delete(h);
        end
        
        function resize(h)
            p = getpixelposition(h.HandleSelf);
            setpixelposition(h.HandleControlPanel,[1 1 p(3) h.ControlHeight]);
            setpixelposition(h.HandleContentPanel,[1 h.ControlHeight p(3) p(4)-h.ControlHeight]);    
        end
        
        
    end
    
end

