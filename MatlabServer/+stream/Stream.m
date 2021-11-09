classdef Stream < handle
    %STREAM Base class for stream objects
    %   Detailed explanation goes here
    
    properties (Access=protected)
        islocked=false;
    end
    
    methods (Abstract=true)
       step(h,d);
    end
    
    methods 
        
        function b = isLocked(h)
            b = h.islocked;
        end
        
    end
    
end

