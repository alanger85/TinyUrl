using System.Runtime.Intrinsics.Arm;
using System.Text;

namespace TinyUrl.Web.Logic
{
    public static class Util
    {
        private const int SHORT_URL_LENGTH = 8;

        private const int CHART_TYPE_DIGIT = 0;
        private const int CHART_TYPE_UPPER_CASE = 1;
        private const int CHART_TYPE_LOWER_CASE = 2;

        public static string GenerateShortUrl()
        {
            Random r = new Random();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < SHORT_URL_LENGTH; i++)
            {
                var chartType =   r.Next(0, 3);
                int asciiNumber = -1;
                switch (chartType)
                {
                    case CHART_TYPE_DIGIT:
                        asciiNumber = (int)r.Next(48, 58);
                        break;

                    case CHART_TYPE_UPPER_CASE:
                        asciiNumber = (int)r.Next(65, 91);
                        break;

                    case CHART_TYPE_LOWER_CASE:
                        asciiNumber = (int)r.Next(97, 123);
                        break;
                }

                sb.Append(Convert.ToChar(asciiNumber));
            }

            return sb.ToString();
        }

    }
}
