using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DateTimeShower : MonoBehaviour
{
    public Text dateText; // Reference to the Text component for displaying the date
    public Text timeText; // Reference to the Text component for displaying the time

    public enum DateFormat
    {
        SystemDefault,
        YearMonthDay,
        DayMonthYear,
        MonthDayYear
    }

    public enum TimeFormat
    {
        SystemDefault,
        HourMinuteSecond,
        HourMinute
    }

    public enum DateSeparator
    {
        Dash, // "-"
        Slash // "/"
    }

    public DateFormat dateFormat = DateFormat.SystemDefault; // Date format selection
    public TimeFormat timeFormat = TimeFormat.SystemDefault; // Time format selection
    public DateSeparator dateSeparator = DateSeparator.Dash; // Date separator selection

    public bool enableTimeSeparatorBlink = false; // Enable or disable blinking of time separator
    private bool isSeparatorVisible = true; // Internal state for blinking
    private float blinkTimer = 0f; // Timer for blinking
    public float blinkInterval = 0.5f; // Interval for blinking in seconds

    // Update is called once per frame
    void Update()
    {
        UpdateDate();
        UpdateTime();
    }

    void UpdateDate()
    {
        if (dateText != null)
        {
            string separator = dateSeparator == DateSeparator.Dash ? "-" : "/";
            string dateFormatString = "";

            switch (dateFormat)
            {
                case DateFormat.SystemDefault:
                    dateText.text = System.DateTime.Now.ToShortDateString();
                    return;
                case DateFormat.YearMonthDay:
                    dateFormatString = $"yyyy{separator}MM{separator}dd";
                    break;
                case DateFormat.DayMonthYear:
                    dateFormatString = $"dd{separator}MM{separator}yyyy";
                    break;
                case DateFormat.MonthDayYear:
                    dateFormatString = $"MM{separator}dd{separator}yyyy";
                    break;
            }

            dateText.text = System.DateTime.Now.ToString(dateFormatString);
        }
    }

    void UpdateTime()
    {
        if (timeText != null)
        {
            if (enableTimeSeparatorBlink)
            {
                blinkTimer += Time.deltaTime;
                if (blinkTimer >= blinkInterval)
                {
                    isSeparatorVisible = !isSeparatorVisible;
                    blinkTimer = 0f;
                }

                string timeString = "";
                switch (timeFormat)
                {
                    case TimeFormat.SystemDefault:
                        timeString = System.DateTime.Now.ToShortTimeString();
                        break;
                    case TimeFormat.HourMinuteSecond:
                        timeString = System.DateTime.Now.ToString("HH:mm:ss");
                        break;
                    case TimeFormat.HourMinute:
                        timeString = System.DateTime.Now.ToString("HH:mm");
                        break;
                }

                if (!isSeparatorVisible)
                {
                    timeString = timeString.Replace(":", " ");
                }

                timeText.text = timeString;
            }
            else
            {
                string timeString = "";
                switch (timeFormat)
                {
                    case TimeFormat.SystemDefault:
                        timeString = System.DateTime.Now.ToShortTimeString();
                        break;
                    case TimeFormat.HourMinuteSecond:
                        timeString = System.DateTime.Now.ToString("HH:mm:ss");
                        break;
                    case TimeFormat.HourMinute:
                        timeString = System.DateTime.Now.ToString("HH:mm");
                        break;
                }
                timeText.text = timeString;
            }
        }
    }
}
