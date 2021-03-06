classdef SignalGenerator < stream.source.Source
    %UNTITLED Summary of this class goes here
    %   Detailed explanation goes here
    
    properties
        SamplesPerFrame=256;
        SampleRate=256;
        SamplesPerTrigger=Inf;
    end
    
    properties (SetAccess=private)
        NumChannels=16;
    end
    
    properties (Dependent=true)
        SampleTime;
        SignalType;
        SamplesAcquiredFcn;
    end
    
    properties (Constant,Hidden=true)
        Random=1;
        Sine=2;
        NoisySine=3;
    end
    
    properties (Access=private)
        type=2;
        index = 0;
        Timer;
        amp=0.0001;
        abstime = [];
    end
    
    methods
        function h = SignalGenerator
            h.Timer = timer;
            h.Timer.ExecutionMode = 'fixedRate';
            h.Timer.BusyMode = 'queue';
        end
        
        function [d,t,a] = step(h)
            h.islocked = true;
            
            startidx = h.index;
            stopidx = h.index + h.SamplesPerFrame;
            
            if ~isinf(h.SamplesPerTrigger)
                if (stopidx > h.SamplesPerTrigger)
                    stopidx = h.SamplesPerTrigger;
                end
            end
            
            t = permute((startidx:stopidx-1)*h.SampleTime,[2 1]);
            
            freq=repmat((1:h.NumChannels)*5, [length(t) 1]);
            d = h.amp.*sin(2.*pi.*freq.*repmat(t,[1 h.NumChannels]));
            
            if ~isinf(h.SamplesPerTrigger)
                if (stopidx==h.SamplesPerTrigger)
                    stopidx = 0;
                    stop(h);
                end
            end
            
            h.index = stopidx;
            
            if (nargout>2), a = h.abstime; end
            
        end
        
        function start(h)
            h.islocked = true;
            h.Timer.Period = 1/(h.SampleRate/h.SamplesPerFrame);
            start(h.Timer);
            h.abstime = clock;
        end
        
        function stop(h)
            h.islocked = false;
            stop(h.Timer);
        end
        
        function set.SamplesAcquiredFcn(d,fcn)
            assert(~d.islocked,'SignalGeneratorError:ObjectLocked','This property cannot be changed whilst the object is locked');
            d.Timer.TimerFcn = fcn;
        end
        
        function set.SamplesPerTrigger(d,t)
            assert(~d.islocked,'SignalGeneratorError:ObjectLocked','This property cannot be changed whilst the object is locked');
            d.SamplesPerTrigger = t;
        end
        
        function n = get.SamplesAcquiredFcn(d)
            n = d.Timer.TimerFcn;
        end
        
        function delete(h)
            %             h.SamplesAcquiredFcn = [];
            if ~isDone(h)
                stop(h);
            end
            delete(h.Timer);
        end
        
        function addChannel(d,idx)
            d.NumChannels = d.NumChannels + length(idx);
        end
        
        function t = get.SignalType(d)
            switch d.type
                case stream.source.SignalGenerator.Random
                    t = 'Random';
                case stream.source.SignalGenerator.Sine
                    t = 'Sine';
                case stream.source.SignalGenerator.NoisySine
                    t = 'NoisySine';
            end
        end
        
        function set.SignalType(d,t)
            assert(~d.islocked,'SignalGenerator:ObjectLocked','This property cannot be changed whilst the object is locked');
            % error checking needed
            d.type = t;
        end
        
        function n = get.NumChannels(d)
            n = d.NumChannels;
        end
        
        function set.NumChannels(d,n)
            assert(~d.islocked,'SignalGenerator:ObjectLocked','This property cannot be changed whilst the object is locked');
            d.NumChannels = n;
        end
        
        function n = get.SampleTime(d)
            n = 1/d.SampleRate;
        end
        
        function set.SampleTime(d,sr)
            assert(~d.islocked,'SignalGenerator:ObjectLocked','This property cannot be changed whilst the object is locked');
            d.SampleRate = 1/sr;
        end
        
        function n = get.SampleRate(d)
            n = d.SampleRate;
        end
        
        function set.SampleRate(d,sr)
            assert(~d.islocked,'SignalGenerator:ObjectLocked','This property cannot be changed whilst the object is locked');
            d.SampleRate = sr;
        end
        
        function b = isDone(d)
            if strcmp(d.Timer.Running,'off'), b = true;
            else b = false; end
        end
        
    end
    
end

