using System;
using System.Collections.Generic;
using System.Text;

namespace VK_RMEbot.Models
{
    public class BotButton
    {
        public string label { get; set; }
        public string color { get; set; }
        public string scenario_id { get; set; }
        public string next_id { get; set; }
        /*
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
        */
    }
}
