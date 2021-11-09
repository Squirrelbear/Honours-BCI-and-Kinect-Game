classdef Source < stream.Stream
    %SOURCE Base class for stream.Source objects
    %   Detailed explanation goes here
    
    properties (Abstract=true)
        SamplesPerFrame;
        SampleRate;
        SampleTime;
        SamplesAcquiredFcn;
    end
    
    properties (Abstract=true,SetAccess=private)
        NumChannels;
    end
    
    methods (Abstract=true)
        start(d);
        stop(d);
        % addChannel
    end
    
end

