classdef StreamObject < handle
    %STREAMOBJECT Base class for stream objects
    %   Detailed explanation goes here
    
    properties (Access=protected)
        islocked=false;
    end
    
    methods (Abstract=true)
%        close(h);
       step(h,d);
    end
    
    methods
        
        function b = isLocked(h)
            b = h.islocked;
        end
        
    end
    
end

