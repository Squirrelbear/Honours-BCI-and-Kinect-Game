sck = java.net.Socket('localhost',25000);

in = java.io.BufferedReader(java.io.InputStreamReader(sck.getInputStream));

idx = 50;

data = cell(1,idx);
for i = 1:idx
    t = in.readLine;
    disp(char(t));
%     data{i} = reshape(sscanf(char(t),'%f,'),8,16);
end

sck.close();