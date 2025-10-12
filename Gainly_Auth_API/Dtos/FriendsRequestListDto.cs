using System.ComponentModel.DataAnnotations;
namespace Gainly_Auth_API.Dtos
{


    public class FriendsRequestListDto
    {
        public Guid FriendshipId { get; set; }
        public Guid FromUserId { get; set; }
        public string FromUsername { get; set; }
        
    }

}



