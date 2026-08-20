using System;
using System.Collections.Generic;

namespace Freezium.Core.Models
{
    public class Anime
    {
        public string ID { get; set; }
        public string type { get; set; }
        public string logo { get; set; }
        public string banner { get; set; }
        public string details_banner { get; set; }
        public string poster { get; set; }
        public string name { get; set; }
        public string overview { get; set; }
        public string overview_short { get; set; }
        public string status { get; set; }
        public string quality { get; set; }
        public int? age { get; set; }
        public List<Genre> genre { get; set; }
        public List<Tag> tag { get; set; }
        public List<SoundGroup> sound_group { get; set; }
        public List<SubtitleGroup> subtitle_group { get; set; }
        public List<object> @class { get; set; }
        public List<object> category { get; set; }
        public List<Country> country { get; set; }
        public List<Language> language { get; set; }
        public double imdb_point { get; set; }
        public int? favorite { get; set; }
        public int? like { get; set; }
        public int? dislike { get; set; }
        public List<Season> seasons { get; set; }
        public List<Avatar> avatar { get; set; }
        public int? total_season { get; set; }
        public List<Studio> studios { get; set; }

        public DateTime Expire { get; set; }
    }

    public class Genre
    {
        public string ID { get; set; }
        public string name { get; set; }
    }

    public class SoundGroup
    {
        public string ID { get; set; }
        public string name { get; set; }
        public string value { get; set; }
        public int? row { get; set; }
    }

    public class Avatar
    {
        public string ID { get; set; }
        public string name { get; set; }
        public string link { get; set; }
        public string avatar_set { get; set; }
        public string who_use { get; set; }
    }

    public class Country
    {
        public string ID { get; set; }
        public string name { get; set; }
    }

    public class Language
    {
        public string ID { get; set; }
        public string name { get; set; }
    }

    public class Episode
    {
        public string ID { get; set; }
        public string name { get; set; }
        public int? number { get; set; }
        public string overview { get; set; }
        public string release { get; set; }
        public int? run_time { get; set; }
        public string quality { get; set; }
        public object part { get; set; }
        public object unified { get; set; }
        public List<object> @class { get; set; }
        public object note { get; set; }
        public object contributed { get; set; }
        public bool? download { get; set; }
        public bool? adult { get; set; }
        public bool? ova { get; set; }
        public bool? filler { get; set; }
        public List<string> sound_group { get; set; }
        public List<object> dubbing_group { get; set; }
        public string banner_link { get; set; }
        public string opening_start { get; set; }
        public string opening_end { get; set; }
        public string next_episode { get; set; }
        public bool? revise { get; set; }
    }

    public class Season
    {
        public string ID { get; set; }
        public string name { get; set; }
        public int? number { get; set; }
        public object overview { get; set; }
        public string release { get; set; }
        public List<Episode> episodes { get; set; }
    }

    public class Studio
    {
        public string ID { get; set; }
        public string name { get; set; }
        public string logo { get; set; }
    }

    public class SubtitleGroup
    {
        public string ID { get; set; }
        public string name { get; set; }
        public int? row { get; set; }
    }

    public class Tag
    {
        public string ID { get; set; }
        public string name { get; set; }
    }
}
