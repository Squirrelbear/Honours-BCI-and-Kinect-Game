function b = bands(pxx,f)
b(1,:) = mean(pxx(f>=4 & f<=7,:));
b(2,:) = mean(pxx(f>=8 & f<=10,:));
b(3,:) = mean(pxx(f>=10 & f<=12,:));
b(4,:) = mean(pxx(f>=8 & f<=12,:));
b(5,:) = mean(pxx(f>=15 & f<=20,:));
b(6,:) = mean(pxx(f>=20 & f<=25,:));
b(7,:) = mean(pxx(f>=15 & f<=25,:));
b(8,:) = mean(pxx(f>=25 & f<=45,:));

frontaltheta = mean(b(1,[3 4]),2);
posterioralpha = mean(b(4,[13 14]),2);

b = b(:);
b = [posterioralpha; frontaltheta; b];
end

