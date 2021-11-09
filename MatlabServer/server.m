function hfig = server


%%%%% UI

hfig = figure('position',[0 0 300 590],'MenuBar','none','NumberTitle','off', ...
    'Name','NeuroGame Service','deletefcn',@(varargin)close);
movegui(hfig,'center');

startbutton = uicontrol('style','pushbutton','position',[10 10 280 50],...
    'string','Start','callback',@(varargin)startservice);

ptcp = uipanel('Parent',hfig,'Title','TCP/IP','units','pixels','Position',[10 70 280 120]);
pfile = uipanel('Parent',hfig,'Title','File','units','pixels','Position',[10 200 280 120]);
pcal = uipanel('Parent',hfig,'Title','Calibration: 0','units','pixels','Position',[10 330 280 120]);
peeg = uipanel('Parent',hfig,'Title','EEG','units','pixels','Position',[10 460 280 120]);

calbutton = uicontrol(pcal,'style','pushbutton','position',[20 60 240 40],...
    'string','Calibrate','callback',@(varargin)localcalibrate);

clearcalbutton = uicontrol(pcal,'style','pushbutton','position',[20 20 240 40],...
    'string','Clear','callback',@(varargin)clearcal);

filenametext = uicontrol(pfile,'style','edit','position',[20 30 240 20], ...
    'string',['data_' strrep(datestr(now, 'ddmmyy HHMMSS'),' ','_')]);
uicontrol(pfile,'style','text','position',[20 60 240 20],'string','Filename:');

% pmh = uicontrol(peeg,'Style','popupmenu',...
%                 'String',{'Signal Generator','Emotiv Epoc'},...
%                 'Value',1,'Position',[20 60 240 20]);

allbuttons = [calbutton,clearcalbutton,startbutton,filenametext];
allpanels = [ptcp,pfile,pcal,peeg];

%%%%% BCI

% src0 = stream.source.SignalGenerator;
src0 = stream.source.EmotivEpoc;

src0.SamplesPerFrame = 128;

% chart0 = stream.sink.ChartPlot;
% chart0.SampleRate = src0.SampleRate;
% chart0.NumChannels = src0.NumChannels;
% chart0.SecondsPerPage = 10;
% chart0.YLim = 0.0002 * [-1 1];

dft0 = stream.transform.DFTSpectrum;
dft0.SampleRate = src0.SampleRate;

% fplot0 = stream.sink.FreqPlot;

file0 = stream.sink.FileWriter;
file0.NumChannels = src0.NumChannels;
file0.SamplesPerFrame = src0.SamplesPerFrame;


%%%% vars

outstream = [];
svrsck = [];
dmin=[];
dmax=[];
ncal = 0;


%%%%% local functions

    function startservice
        
        
        
        src0.SamplesAcquiredFcn = @(varargin)process;
        
        svrsck = java.net.ServerSocket(25000);
        svrsck.setSoTimeout(10000);
        
        disp(svrsck.getInetAddress().toString());
        
        disableall;
        set(startbutton,'string','Service Running....');
        
        pause(0.2);
        
        try
            outsck = svrsck.accept();
            
        catch ex
            errordlg('Port Timeout!');
            svrsck.close();
            set(startbutton,'string','Start');
            enableall;
            return;
        end
        
        outstream = outsck.getOutputStream();
        
        start;
        
    end

    function close
        stop;
        delete(src0);
        %         delete(chart0);
        delete(dft0);
        %         delete(fplot0);
    end

    function start
        file0.open(get(filenametext,'string'));
        src0.start;
    end

    function stop
        
        src0.stop;
        file0.close;
    end

    function process
        [d,t] = step(src0);
        %         step(chart0,d);
        [pxx,f] = step(dft0,d);
        %         step(fplot0,pxx,f);
        b = bands(pxx,f);
        if (ncal>0)
            b = (b - dmin) ./ dmax;
        end
        str = sprintf('%.10f,',b);
        str = sprintf('%s\n',str(1:end-1));
        try
            outstream.write(int8(str));
        catch
            stop;
            outstream.close();
            svrsck.close();
            startservice;
        end
        step(file0,d,t);
    end

    function localcalibrate
        disableall;
        [dmin0,dmax0] = calibrate(src0,dft0);
        if (ncal>0)
            dmin = min(dmin0,dmin);
            dmax = max(dmax0,dmax);
        else
            dmin = dmin0;
            dmax = dmax0;
        end
        ncal = ncal + 1;
        set(pcal,'Title',sprintf('Calibration: %i',ncal));
        enableall;
    end

    function clearcal
        dmin=[];
        dmax=[];
        ncal = 0;
        set(pcal,'Title',sprintf('Calibration: %i',ncal));
    end

    function disableall
        set(allbuttons,'Enable','off');
  
    end

    function enableall
        set(allbuttons,'Enable','on');
    end


end