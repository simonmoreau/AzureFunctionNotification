using System;
using System.Collections.Generic;

namespace Notifications
{
    public class AvailableSlot
    {
        public string name {get;set;}
        public string url {get;set;}
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Step
    {
        public int agenda_id { get; set; }
        public object practitioner_agenda_id { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public int visit_motive_id { get; set; }
    }

    public class Slot
    {
        public int agenda_id { get; set; }
        public object practitioner_agenda_id { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public List<Step> steps { get; set; }
    }

    public class Availability
    {
        public string date { get; set; }
        public List<Slot> slots { get; set; }
        public object substitution { get; set; }
    }

    public class Position
    {
        public double lat { get; set; }
        public double lng { get; set; }
    }

    public class SearchResult
    {
        public int id { get; set; }
        public bool is_directory { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string zipcode { get; set; }
        public string link { get; set; }
        public string cloudinary_public_id { get; set; }
        public int profile_id { get; set; }
        public object exact_match { get; set; }
        public bool priority_speciality { get; set; }
        public object first_name { get; set; }
        public string last_name { get; set; }
        public string name_with_title { get; set; }
        public object speciality { get; set; }
        public string organization_status { get; set; }
        public List<string> top_specialities { get; set; }
        public Position position { get; set; }
        public object place_id { get; set; }
        public bool telehealth { get; set; }
        public int visit_motive_id { get; set; }
        public string visit_motive_name { get; set; }
        public List<int> agenda_ids { get; set; }
        public string landline_number { get; set; }
        public bool booking_temporary_disabled { get; set; }
        public bool resetVisitMotive { get; set; }
        public bool toFinalizeStep { get; set; }
        public bool toFinalizeStepWithoutState { get; set; }
        public string url { get; set; }
    }

    public class SearchResponse
    {
        public List<Availability> availabilities { get; set; }
        public int total { get; set; }
        public object message { get; set; }
        public SearchResult search_result { get; set; }
    }


}