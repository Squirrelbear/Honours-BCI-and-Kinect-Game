function [dmin,dmax] = calibrate(src0,dft0)

src0.SamplesAcquiredFcn = @(varargin)process;

t0 = timer('TimerFcn',@(varargin)src0.stop,...
    'StartFcn',@(varargin)src0.start, ...
    'StartDelay',10);

dmin = [];
dmax = [];

start(t0);
wait(t0);

delete(t0);

    function process
        
        [d,~] = step(src0);
        [pxx,f] = step(dft0,d);
        b = bands(pxx,f);
        
        if isempty(dmin)
            dmin = b;
            dmax = b;
            return;
        end
        
        dmin = min(dmin,b);
        dmax = max(dmax,b);
        
        
    end

end