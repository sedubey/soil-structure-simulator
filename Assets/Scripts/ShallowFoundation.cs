using System;

public class ShallowFoundation
{
    public static double[,] Terzaghi =  // Table of values for different degrees
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

    public static bool GetNum(int deg, out double Nc, out double Nq, out double Ny)   // Assigns Nc, Nq, Ny based off degree
    {
        for (int i = 0; i < Terzaghi.GetLength(0); i++) // Loop through table
        {
            if (deg == (int)Terzaghi[i, 0]) // Find degree, assign values
            {
                Nc = Terzaghi[i, 1];
                Nq = Terzaghi[i, 2];
                Ny = Terzaghi[i, 3];
                return true;
            }
        }

        Nc = 0; Nq = 0; Ny = 0; // Otherwise
        return false;
    }

    public static double BearingCapacity(int deg, double c, double q, double y, double b)
    {
        GetNum(deg, out double Nc, out double Nq, out double Ny);
        return (c * Nc) + (q * Nq) + (0.5 * y * b * Ny);    // Bearing capacity equation (same as Strip footing might delete)
    }

    public static double StripFootingDrained(int deg, double c, double q, double y, double b)
    {
        GetNum(deg, out double Nc, out double Nq, out double Ny);
        return (c * Nc) + (q * Nq) + (0.5 * y * b * Ny);    // Strip footing equation
    }

    public static double StripFootingUndrained(double Cu, double q)
    {
        return (5.7 * Cu) + q;  // Strip footing equation (Undrained)
    }

    public static double SquareFootingDrained(int deg, double c, double Sc, double q, double y, double b, bool isSquare)
    {
        GetNum(deg, out double Nc, out double Nq, out double Ny);

        double Sy;

        if (isSquare)   // Sy is different based on shape
            Sy = 0.6;
        else
            Sy = 0.8;

        return (c * Nc * Sc) + (q * Nq) + (0.5 * y * b * Ny * Sy);  // Square/Circle equation
    }

    public static double SquareFootingUndrained(double Cu, double Sc, double q)
    {
        return (5.7 * Cu * Sc) + q; // Square/Circle equation
    }
}