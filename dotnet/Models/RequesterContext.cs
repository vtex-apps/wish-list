namespace WishList.Models
{
    public class RequesterContext
    {
        public bool IsAuthenticated { get; set; }
        public bool IsAdmin { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
    }
}
