using System;

public class ShallowFoundation
{
    public static double[,] Terzaghi =
    {
        {0, 5.70, 1.00, 0.00},
        {10, 9.61, 2.69, 0.56},
        {12, 10.76, 3.29, 0.85},
        {14, 12.11, 4.02, 1.26},
        {16, 13.68, 4.92, 1.82},
        {18, 15.12, 6.04, 2.59},
        {20, 17.69, 7.44, 3.64},
        {22, 20.27, 9.19, 5.09},
        {24, 23.36, 11.40, 7.08},
        {26, 27.09, 14.21, 9.84},
        {28, 31.61, 17.81, 13.70},
        {30, 37.16, 22.46, 19.13},
        {32, 44.04, 28.52, 26.87},
        {34, 52.64, 36.50, 38.04},
        {36, 63.53, 47.16, 54.36},
        {38, 77.50, 61.55, 78.61},
        {40, 95.66, 81.27, 115.31},
        {42, 119.67, 108.75, 171.99},
        {44, 151.95, 147.74, 261.60},
        {46, 196.22, 204.19, 407.11}
    };

    public static bool GetNum(int deg, out double Nc, out double Nq, out double Ny)
    {
        for (int i = 0; i < Terzaghi.GetLength(0); i++)
        {
            if (deg == (int)Terzaghi[i, 0])
            {
                Nc = Terzaghi[i, 1];
                Nq = Terzaghi[i, 2];
                Ny = Terzaghi[i, 3];
                return true;
            }
        }

        Nc = 0; Nq = 0; Ny = 0;
        return false;
    }

    // Overburden pressure = unit weight * depth. For surface foundations, depth = 0, so overburden = 0
    public static double BearingCapacity(int deg, double c, double overburdenPressure, double y, double b)
    {
        GetNum(deg, out double Nc, out double Nq, out double Ny);
        return (c * Nc) + (overburdenPressure * Nq) + (0.5 * y * b * Ny);
    }

    public static double StripFootingDrained(int deg, double c, double overburdenPressure, double y, double b)
    {
        GetNum(deg, out double Nc, out double Nq, out double Ny);
        return (c * Nc) + (overburdenPressure * Nq) + (0.5 * y * b * Ny);
    }

    public static double StripFootingUndrained(double Cu)
    {
        return (5.7 * Cu);  // Removed + q since q is overburden, assumed 0 for surface
    }

    public static double SquareFootingDrained(int deg, double c, double Sc, double overburdenPressure, double y, double b, bool isSquare)
    {
        GetNum(deg, out double Nc, out double Nq, out double Ny);

        double Sy;
        if (isSquare)
            Sy = 0.6;
        else
            Sy = 0.8;

        return (c * Nc * Sc) + (overburdenPressure * Nq) + (0.5 * y * b * Ny * Sy);
    }

    public static double SquareFootingUndrained(double Cu, double Sc)
    {
        return (5.7 * Cu * Sc);  // Removed + q
    }
}