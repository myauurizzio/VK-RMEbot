using System;
using System.Collections.Generic;
using System.Text;

namespace VK_RMEbot.Models
{
    public class Scenario
    {
        public string scenario_id { get; set; }
        public string scenario_name { get; set; }
        //public string file_name { get; set; }
        public string start_id { get; set; }
        public List<Step> steps { get; set; }
        /*    
"scenario_name": "Problem Solver",
"start_id": "1000",
"steps": [
    {
        "id": "1000",
        "text": "Всё нормально?",
        "buttons": [
          {
            "label": "Да",
            "color": "positive",
            "next_id": "1100"
          },
          {
            "label": "Нет",
            "color": "negative",
            "next_id": "1200"
          }
        ]
    },
*/

    }
}
