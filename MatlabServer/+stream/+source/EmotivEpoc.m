classdef EmotivEpoc < stream.source.Source
    %EmotivEpoc Source class for Emotiv Epoc EEG headset
    %   Detailed explanation goes here
    
    properties %(Access=private)
        engine = [];
        asmb = [];
        userid = [];
        Timer;
        buffer = [];
    end
    
    properties (SetAccess=private)
        NumChannels = 16;
        
    end
    
    properties
        SamplesPerFrame;  
        SamplesAcquiredFcn;
    end
    
    properties (Dependent)
        BufferSize;
        SampleRate;
        
        SampleTime;
    end
    
    methods
        
        function d = EmotivEpoc
            
            d.Timer = timer;
            d.Timer.ExecutionMode = 'fixedRate';
            d.Timer.BusyMode = 'queue';
            d.Timer.TimerFcn = @(varargin)sampleevent(d);
            
            p = what('stream');
            d.asmb = NET.addAssembly(fullfile(p.path,'+source','private','DotNetEmotivSDK.dll'));
            d.engine = Emotiv.EmoEngine.Instance;
            
            addlistener(d.engine,'UserAdded',@useraddedlistener);
            d.engine.Connect;
            
            while isempty(d.userid)
                d.engine.ProcessEvents;
            end
            
            function useraddedlistener(~,a)
                d.userid = a.userId;
                d.BufferSize = 5;
                d.SamplesPerFrame = 0.25 * d.SampleRate;
            end
        end
        
        function start(d)
            d.islocked = true;
            try
                d.engine.DataAcquisitionEnable(d.userid, true);
%                 period = d.SamplesPerFrame/d.SampleRate;
%                 period = floor(period*1000)/1000;
                period = 1/10;
                d.Timer.Period = period;
                d.Timer.StartDelay = period;
                start(d.Timer);
            catch ex
                d.islocked = false;
                throw(ex);
            end
        end
        
        function stop(d)
            d.islocked = false;
            stop(d.Timer);
            d.engine.DataAcquisitionEnable(d.userid, false);
        end
        
        function set.SamplesAcquiredFcn(d,fcn)
            assert(~d.islocked,'EmotivEpoc:ObjectLocked','This property cannot be changed whilst the object is locked');
            
            d.SamplesAcquiredFcn = fcn;
        end
        
        function sampleevent(d)
           data = d.engine.GetData(d.userid);
           
           if ~isempty(data)
                
                t = double(data.Item(Emotiv.EE_DataChannel_t.TIMESTAMP));
                
                v = nan(16,length(t));
                
                v(1,:) = double(data.Item(Emotiv.EE_DataChannel_t.AF3));
                v(2,:) = double(data.Item(Emotiv.EE_DataChannel_t.AF4));
                v(3,:) = double(data.Item(Emotiv.EE_DataChannel_t.F3));
                v(4,:) = double(data.Item(Emotiv.EE_DataChannel_t.F4));
                v(5,:) = double(data.Item(Emotiv.EE_DataChannel_t.F7));
                v(6,:) = double(data.Item(Emotiv.EE_DataChannel_t.F8));
                v(7,:) = double(data.Item(Emotiv.EE_DataChannel_t.FC5));
                v(8,:) = double(data.Item(Emotiv.EE_DataChannel_t.FC6));
                v(9,:) = double(data.Item(Emotiv.EE_DataChannel_t.T7));
                v(10,:) = double(data.Item(Emotiv.EE_DataChannel_t.T8));
                v(11,:) = double(data.Item(Emotiv.EE_DataChannel_t.P7));
                v(12,:) = double(data.Item(Emotiv.EE_DataChannel_t.P8));
                v(13,:) = double(data.Item(Emotiv.EE_DataChannel_t.O1));
                v(14,:) = double(data.Item(Emotiv.EE_DataChannel_t.O2));
                v(15,:) = double(data.Item(Emotiv.EE_DataChannel_t.GYROX));
                v(16,:) = double(data.Item(Emotiv.EE_DataChannel_t.GYROY));
                v = v'./1000;
                
                v = [t' v];
                
                d.buffer = [d.buffer; v]; %% OPTIMISE THIS RUBBISH
           end
            
           if size(d.buffer,1)>d.SamplesPerFrame
                 d.SamplesAcquiredFcn(); 
           end
        end
        
%         function f = get.SamplesAcquiredFcn(d)
%             f = d.Timer.TimerFcn;
%         end
        
        function [v,t] = step(d)
            v=[]; t=[];
            
            
            
            if ~isempty(d.buffer)
                
                b0 = d.buffer(1:d.SamplesPerFrame,:);
                d.buffer(1:d.SamplesPerFrame,:)=[];
                
                v = b0(:,2:end);
                t = b0(:,1:end);
            end
            
%             v = []; t = [];
%             
%             data = d.engine.GetData(d.userid);
%             
%             if ~isempty(data)
%                 
%                 t = double(data.Item(Emotiv.EE_DataChannel_t.TIMESTAMP));
%                 
%                 v = nan(16,length(t));
%                 
%                 v(1,:) = double(data.Item(Emotiv.EE_DataChannel_t.AF3));
%                 v(2,:) = double(data.Item(Emotiv.EE_DataChannel_t.AF4));
%                 v(3,:) = double(data.Item(Emotiv.EE_DataChannel_t.F3));
%                 v(4,:) = double(data.Item(Emotiv.EE_DataChannel_t.F4));
%                 v(5,:) = double(data.Item(Emotiv.EE_DataChannel_t.F7));
%                 v(6,:) = double(data.Item(Emotiv.EE_DataChannel_t.F8));
%                 v(7,:) = double(data.Item(Emotiv.EE_DataChannel_t.FC5));
%                 v(8,:) = double(data.Item(Emotiv.EE_DataChannel_t.FC6));
%                 v(9,:) = double(data.Item(Emotiv.EE_DataChannel_t.T7));
%                 v(10,:) = double(data.Item(Emotiv.EE_DataChannel_t.T8));
%                 v(11,:) = double(data.Item(Emotiv.EE_DataChannel_t.P7));
%                 v(12,:) = double(data.Item(Emotiv.EE_DataChannel_t.P8));
%                 v(13,:) = double(data.Item(Emotiv.EE_DataChannel_t.O1));
%                 v(14,:) = double(data.Item(Emotiv.EE_DataChannel_t.O2));
%                 v(15,:) = double(data.Item(Emotiv.EE_DataChannel_t.GYROX));
%                 v(16,:) = double(data.Item(Emotiv.EE_DataChannel_t.GYROY));
%                 v = v'./1000;
%             end
%             
        end
        
        function set.BufferSize(d,s)
             assert(~d.islocked,'EmotivEpoc:ObjectLocked','This property cannot be changed whilst the object is locked');
            d.engine.EE_DataSetBufferSizeInSec(s);
        end
        
        function s = get.BufferSize(d)
            s = d.engine.EE_DataGetBufferSizeInSec;
        end
        
        function delete(d)
            d.engine.Disconnect;
            delete@stream.Stream(d);
        end
        
        function s = get.SampleRate(d)
            s = double(d.engine.DataGetSamplingRate(d.userid));
        end

        function s = get.SampleTime(d)
           s = 1/d.SampleRate; 
        end
        
        function b = isDone(d)
            if strcmp(d.Timer.Running,'off'), b = true;
            else b = false; end
        end
        
    end
    
end

