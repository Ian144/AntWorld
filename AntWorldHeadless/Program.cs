var antWorldSeq = AntWorldEntryPoint.MakeAntWorldSeq(256, 1, 32, 64, 256, 0);

var ctr = 0;
foreach (var _ in antWorldSeq)
{
    ctr++;
    if (ctr % 1000 == 0) Console.WriteLine(ctr);
}

