namespace EonWatchesAPI.DbContext;

public class Favourites
{
    public int Id { get; set; }
    public Ad Ad { get; set; }
    public DateTime FavouriteAddedAt { get; set; }
}