using System;
using System.Collections.Generic;
using System.Text;

namespace VK_RMEbot.Models
{
    public class Step
    {
        public string id { get; set; }
        public string text { get; set; }
        public List<BotButton> buttons { get; set; }
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
