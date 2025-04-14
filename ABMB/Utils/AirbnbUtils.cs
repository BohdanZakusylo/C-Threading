namespace ABMB.Controllers.AirbnbModule;

   public class AirbnbUtils{

    public (int Year, int Month) GetPreviousMonth(int year, int currentMonth)
    {
        if (currentMonth == 1)
        {
            return (year - 1, 12);
        }
        return (year, currentMonth - 1);
    }

    public decimal CalculatePercentageDifference(decimal currentPrice, decimal previousPrice)
    {
        if (previousPrice == 0)
                {
                    if (currentPrice == 0)
                        return 0;
                    else
                        return 100; 
                }

        return ((currentPrice - previousPrice) / previousPrice) * 100;
    }
   }
    
   