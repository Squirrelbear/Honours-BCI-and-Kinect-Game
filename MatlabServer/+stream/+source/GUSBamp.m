classdef gUSBamp < stream.StreamObject
    %gUSBamp Biosignal data acquisition
    %   Class for acquisition of biosignal data from Guger Technologies
    %   gUSBamp device
    
    properties %(SetAccess=private)
       analog=[];
    end
    
    properties (Dependent=true)
        SamplesPerFrame;
        NumChannels;
        SampleTime;
        TriggerType;
        SamplesPerTrigger;
        SampleRate;
        Mode;
        SamplesAcquiredFcn;
        TriggerFcn;
        TriggerRepeat;
    end
    
    methods
        
        function d = gUSBamp  
            h = daqhwinfo;
            idx = strcmp(h.InstalledAdaptors,'guadaq');
            
            assert(any(idx),'gUSBampError:AdaptorNotFound', ...
                'g.USBamp adaptor not found/installed');
            
            h = daqhwinfo('guadaq');
            
            assert(~isempty(h.InstalledBoardIds), ...
                'gUSBampError:DeviceNotFound', ...
                'g.USBamp device not found/connected');
            
            d.analog = eval(h.ObjectConstructorName{1});
            d.analog.SamplesPerTrigger = Inf;
%             d.analog.DataMissedFcn = @stop;
            d.SamplesPerFrame = 1;
        end
        
        function n = get.NumChannels(d)
            n = length(d.analog.Channel);
        end
        
        function addChannel(d,idx)
           addchannel(d.analog,idx); 
        end
        
        function set.SamplesPerFrame(d,n)
           d.analog.SamplesAcquiredFcnCount = n; 
        end
        
        function n = get.SamplesPerFrame(d)
           n = d.analog.SamplesAcquiredFcnCount; 
        end
        
        function n = get.SampleTime(d)
            n = 1/d.analog.SampleRate;
        end
        
        function set.SampleTime(d,sr)
            assert(~d.islocked,'gUSBampError:ObjectLocked','This property cannot be changed whilst the object is locked');
            assert(ismember(sr,[1/32 1/64 1/128 1/256 1/512 1/600 1/1200 1/1400 1/4800]), ...
                'gUSBampError:InvalidSampleRate', ...
                'Sample rate must be [1/32 1/64 1/128 1/256 1/512 1/600 1/1200 1/1400 1/4800]');
            d.analog.SampleRate = 1/sr;
        end
        
        function n = get.SampleRate(d)
            n = d.analog.SampleRate;
        end
        
        function set.SampleRate(d,sr)
            assert(~d.islocked,'gUSBampError:ObjectLocked','This property cannot be changed whilst the object is locked');
            assert(ismember(sr,[32 64 128 256 512 600 1200 1400 4800]), ...
                'gUSBampError:InvalidSampleRate', ...
                'Sample rate must be [32 64 128 256 512 600 1200 1400 4800]');
            d.analog.SampleRate = sr;
        end
        
        function set.SamplesAcquiredFcn(d,fcn)
            assert(~d.islocked,'gUSBampError:ObjectLocked','This property cannot be changed whilst the object is locked');
            d.analog.SamplesAcquiredFcn = fcn;
        end
        
        function set.TriggerFcn(d,fcn)
            assert(~d.islocked,'gUSBampError:ObjectLocked','This property cannot be changed whilst the object is locked');
            d.analog.TriggerFcn = fcn;
        end
        
        function n = get.SamplesAcquiredFcn(d)
            n = d.analog.SamplesAcquiredFcn;
        end
        
        function n = get.Mode(d)
            n = d.analog.Mode;
        end
        
        function set.Mode(d,m)
            assert(~d.islocked,'gUSBampError:ObjectLocked','This property cannot be changed whilst the object is locked');
            assert(ismember(m,{'Normal' 'Impedance' 'Calibration'}), ...
                'gUSBampError:InvalidMode', ...
                'Mode must be ''Normal'', ''Impedance'', or ''Calibration''');
            d.analog.Mode = m;
        end
        
        function set.TriggerType(d,t)
            assert(~d.islocked,'gUSBampError:ObjectLocked','This property cannot be changed whilst the object is locked');
            d.analog.TriggerType = t;
        end
        
        function set.TriggerRepeat(d,r)
            assert(~d.islocked,'gUSBampError:ObjectLocked','This property cannot be changed whilst the object is locked');
            d.analog.TriggerRepeat = r;
        end
        
        function n = get.TriggerType(d)
            n = d.analog.TriggerType;
        end
        
        function set.SamplesPerTrigger(d,t)
            assert(~d.islocked,'gUSBampError:ObjectLocked','This property cannot be changed whilst the object is locked');
            d.analog.SamplesPerTrigger = t;
        end
        
        function n = get.SamplesPerTrigger(d)
            n = d.analog.SamplesPerTrigger;
        end
        
        function trigger(d)
            assert(strcmpi(d.TriggerType,'manual'), ...
                'gUSBampError:InvalidTriggerType', ...
                'TriggerType must be set to ''manual'' to use trigger method');
            if isDone(d), start(d); end
            trigger(d.analog);
        end
        
        function start(d)
            start(d.analog);
        end
        
        function stop(d)
            stop(d.analog);
        end
        
        function [data,time,abstime] = step(d)
            h.islocked = true;
            [data,time,abstime] = getdata(d.analog,d.SamplesPerFrame);
        end
        
        function b = isDone(d)
            if strcmp(d.analog.running,'Off'), b = true;
            else b = false; end
        end
        
        function delete(h)
%             if isprop(h,'SamplesAcquiredFcn'), h.SamplesAcquiredFcn = []; end
            if ~isempty(h.analog)  && ~isDone(h)
                stop(h);
            end
            delete(h.analog);
        end
        
%         function close(h)
%             delete(h);
%         end
        
    end
    
end

