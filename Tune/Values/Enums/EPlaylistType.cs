namespace Tune.Values.Enums
{
    public enum EPlaylistType
    {
        Empty = 0,
        Collection = 1,
        Directory = 2,
        // CollectionAndDirectory = 3,
        // **NOTE**: We can hold this as an update: 
        //           This type allows user add music
        //           to a Directory type playlist
    }
}
