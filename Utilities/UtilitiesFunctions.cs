using AuthenticationService.Interfaces;
namespace AuthenticationService.Utilities
{
    public static class UtilitiesFunctions
    {
        public static DateTime ExpirationToken(int daysExpire)
        {
            return DateTime.UtcNow.AddMinutes(daysExpire);
        }

        public static Response GetElementHeader(IHeaderDictionary headers, string authorizationHeader)
        {
            var elementHeader = headers[authorizationHeader].ToString();
            if (elementHeader is null)
            {
                return new Response(false, $"The {authorizationHeader} Header is missing", null);
            }

            Response elementToReturn = new Response(false, "", null);

            switch (authorizationHeader)
            {
                case "Authorization":
                    elementToReturn.Success = true;
                    elementToReturn.Data = "Token successfully recovered";
                    elementToReturn.Data = elementHeader.Replace("Bearer ", "");
                    break;
                default:
                    elementToReturn.Success = true;
                    elementToReturn.Data = $"{authorizationHeader} successfully recovered";
                    elementToReturn.Data = elementHeader;
                    break;
            }
            return elementToReturn;
        }

    }
}