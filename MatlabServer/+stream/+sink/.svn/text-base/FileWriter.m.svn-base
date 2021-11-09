classdef FileWriter <  stream.StreamObject
    %UNTITLED Summary of this class goes here
    %   Detailed explanation goes here
    
    properties
        NumChannels = 1;
        SamplesPerFrame = 1;
        AbsTime = 0;
        Path = '.';
    end
    
    properties (Access=private)
       hid
       did
    end

    methods
        
        function d = FileWriter
        end
        
        function step(h,d,t)
            assert(size(d,1)==h.SamplesPerFrame, ...
                'FileWriterError:InvalidSamplesPerFrame', ...
                'Frame size (rows) does not match SamplesPerFrame property');
            
            assert(size(d,2)==h.NumChannels, ...
                'FileWriterError:InvalidNumChannels', ...
                'Number of channels (cols) does not match NumChannels property');
            
            fwrite(h.did,[t,d]','double');
            
        end
        
%         function set.ChannelLabels(h,n)
%            assert(iscellstr(n),'FileWriterError:InvalidType','This property should be a cellstring');
%            h.ChannelLabels = n;
%         end
        
        function set.Path(h,n)
            assert(~h.islocked,'FileWriterError:ObjectLocked','This property cannot be changed whilst the object is locked');
            h.Path = n;
        end

        function set.NumChannels(h,n)
            assert(~h.islocked,'FileWriterError:ObjectLocked','This property cannot be changed whilst the object is locked');
            h.NumChannels = n;
        end
        
        function set.SamplesPerFrame(h,n)
            assert(~h.islocked,'FileWriterError:ObjectLocked','This property cannot be changed whilst the object is locked');
            h.SamplesPerFrame = n;
        end
        
        function close(h)
           if ~h.islocked, return; end
           fprintf(h.hid,'''Channels'':%i,''SamplesPerFrame'':%i,''AbsTime'':%i', ...
               h.NumChannels,h.SamplesPerFrame,datenum(h.AbsTime));
           fclose(h.did);
           fclose(h.hid);
           h.islocked = false;
        end
        
        function open(h,fname)
            h.islocked = true;
            fname = fullfile(h.Path,fname);
            h.hid = fopen([fname '.hdr'],'w');
            h.did = fopen([fname '.bin'],'w');
        end
        
        
    end
    
end

