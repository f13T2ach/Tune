using Newtonsoft.Json.Linq;

using Tune.Helpers.JSONConverters;
using Tune.Models.Playlist;
using Tune.Models.Results;
using Tune.Tests.Helpers.Mock;
using Xunit;

namespace Tune.Tests.Helpers.JSONConverters
{
    /// <summary>Verifies the behavior of <see cref="PlaylistBaseConverter"/>.</summary>
    public class PlaylistBaseConverterTests
    {
        #region ==== Serialization ====

        /// <summary>Verifies that the playlist fields are copied to the JSON object.</summary>
        [Fact]
        public void FromCollectionPlaylistToJSON_WithPlaylist_CopiesMetadata()
        {
            var playlist = new CollectionPlaylist("id-1", "My List", "Desc");

            JObject json = PlaylistBaseConverter.FromCollectionPlaylistToJSON(playlist)!;

            Assert.Equal("id-1", json["ID"]!.Value<string>());
            Assert.Equal("My List", json["Name"]!.Value<string>());
            Assert.Equal("Desc", json["Description"]!.Value<string>());
            Assert.Equal("Collection", json["Type"]!.Value<string>());
            Assert.False(json["IsDeleted"]!.Value<bool>());
        }

        /// <summary>Verifies that each music entry is stored as a path inside an array.</summary>
        [Fact]
        public void FromCollectionPlaylistToJSON_WithMusic_StoresMusicPaths()
        {
            var playlist = new CollectionPlaylist("id-1", "My List", "Desc");
            playlist.AddMuisc(MusicTestFactory.CreateFake("Title A", "Artist A"));

            JObject json = PlaylistBaseConverter.FromCollectionPlaylistToJSON(playlist)!;

            JArray musicList = Assert.IsType<JArray>(json["MusicList"]);
            Assert.Equal(JTokenType.String, Assert.Single(musicList).Type);
        }

        /// <summary>Verifies that a missing source is serialized as a null token.</summary>
        [Fact]
        public void FromCollectionPlaylistToJSON_WithoutSource_WritesNullSource()
        {
            var playlist = new CollectionPlaylist("id-1", "My List", "Desc");

            JObject json = PlaylistBaseConverter.FromCollectionPlaylistToJSON(playlist)!;

            Assert.Equal(JTokenType.Null, json["SourceFrom"]!.Type);
        }

        #endregion

        #region ==== Round Trip ====

        /// <summary>Verifies that a playlist survives a serialize and deserialize round trip.</summary>
        [Fact]
        public void FromJSONToBase_AfterSerialization_ReturnsEquivalentPlaylist()
        {
            var playlist = new CollectionPlaylist("id-1", "My List", "Desc");
            playlist.AddMuisc(MusicTestFactory.CreateFake("Title A", "Artist A"));

            ResultContentWithException<PlaylistBase> result = PlaylistBaseConverter.FromJSONToBase(playlist.ToJObject()!);

            Assert.True(result.Success);
            Assert.Equal("id-1", result.Content.ID);
            Assert.Equal("My List", result.Content.Name);
            Assert.Equal("Desc", result.Content.Description);
            Assert.Single(result.Content.MusicList);
        }

        #endregion

        #region ==== Deserialization ====

        /// <summary>Verifies that a collection type is recognized regardless of letter case.</summary>
        [Theory]
        [InlineData("Collection")]
        [InlineData("collection")]
        public void FromJSONToBase_WithCollectionType_ReturnsCollectionPlaylist(string type)
        {
            var raw = new JObject { ["Type"] = type, ["ID"] = "id-1", ["Name"] = "My List" };

            ResultContentWithException<PlaylistBase> result = PlaylistBaseConverter.FromJSONToBase(raw);

            Assert.True(result.Success);
            Assert.IsType<CollectionPlaylist>(result.Content);
        }

        /// <summary>Verifies that an unsupported type is reported as a failure.</summary>
        [Fact]
        public void FromJSONToBase_WithUnsupportedType_ReportsFailure()
        {
            var raw = new JObject { ["Type"] = "Directory" };

            ResultContentWithException<PlaylistBase> result = PlaylistBaseConverter.FromJSONToBase(raw);

            Assert.False(result.Success);
            Assert.IsType<NotSupportedException>(result.Exception);
        }

        /// <summary>Verifies that a missing type field is reported as a failure.</summary>
        [Fact]
        public void FromJSONToBase_WithoutTypeField_ReportsFailure()
        {
            ResultContentWithException<PlaylistBase> result = PlaylistBaseConverter.FromJSONToBase(new JObject());

            Assert.False(result.Success);
            Assert.IsType<InvalidOperationException>(result.Exception);
        }

        /// <summary>Verifies that a null JSON object is reported as a failure.</summary>
        [Fact]
        public void FromJSONToBase_WithNullObject_ReportsFailure()
        {
            ResultContentWithException<PlaylistBase> result = PlaylistBaseConverter.FromJSONToBase(null!);

            Assert.False(result.Success);
            Assert.IsType<ArgumentNullException>(result.Exception);
        }

        /// <summary>Verifies that a null entry inside the music array is skipped.</summary>
        [Fact]
        public void FromJSONToCollectionPlaylist_WithNullEntry_SkipsIt()
        {
            var raw = new JObject
            {
                ["ID"] = "id-1",
                ["Name"] = "My List",
                ["MusicList"] = new JArray { JValue.CreateNull(), @"C:\Fake\a.mp3" }
            };

            ResultContentWithException<PlaylistBase> result = PlaylistBaseConverter.FromJSONToCollectionPlaylist(raw);

            Assert.True(result.Success);
            Assert.Single(result.Content.MusicList);
        }

        /// <summary>Verifies that a missing music array yields an empty list.</summary>
        [Fact]
        public void FromJSONToCollectionPlaylist_WithoutMusicArray_ReturnsEmptyList()
        {
            var raw = new JObject { ["ID"] = "id-1", ["Name"] = "My List" };

            ResultContentWithException<PlaylistBase> result = PlaylistBaseConverter.FromJSONToCollectionPlaylist(raw);

            Assert.True(result.Success);
            Assert.Empty(result.Content.MusicList);
        }

        #endregion
    }
}
