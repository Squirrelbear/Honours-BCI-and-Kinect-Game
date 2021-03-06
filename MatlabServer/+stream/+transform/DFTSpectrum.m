classdef DFTSpectrum < handle
    %DFTSpectrum Power Spectrum
    %   Calculate the power spectrum using the Discrete Fourier Transform
    
    properties (Access=private)
        fftobj;
    end
    
    properties
        SampleRate = 1;
        Window = @hamming;
    end
    
    properties (Dependent=true)
        SampleTime;
    end
    
    methods
        function d = DFTSpectrum
            d.fftobj = dsp.FFT;
            d.fftobj.FFTImplementation = 'FFTW';
        end
        
        function set.SampleRate(H,D)
            H.SampleRate = D;
        end
        
        function D = get.SampleRate(H)
            D = H.SampleRate;
        end
        
        function set.SampleTime(H,D)
            H.SampleRate = 1/D;
        end
        
        function D = get.SampleTime(H)
            D = 1/H.SampleRate;
        end
        
        
        function [Pxx,f] = step(H,D)
            Nblock = size(D,1);
            Nf = Nblock / 2 + 1;
            
            D = D .* repmat(window(H.Window,Nblock),[1 size(D,2)]);
            
            Pxx = step(H.fftobj,D);
            Pxx = abs(Pxx).^2;
            Pxx(Nf+1:end,:) = [];
            
            f = (( 0:( Nf - 1)) * H.SampleRate / Nblock)';
        end
        
        function delete(h)
            delete(h.fftobj);
            delete@handle(h);
        end
    end
    
end

