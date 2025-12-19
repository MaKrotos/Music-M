// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#pragma once

namespace wv2winrt_impl
{
// The bulk of this code is copied and simplified from base::Time.

// In cpp/winrt the DateTime and TimeSpan classes are simply type specializations
// of std::chrono::time_point and std::chrono::duration, respectively. They use
// 100-nanosecond ticks and the Windows epoch (1/1/1601).

// Offset in days of the VARIANT DATE epoch (1899-12-30 00:00:00 UTC) from the
// Windows epoch (1601-01-01 00:00:00 UTC). This value is derived from the
// following:
// ((1899-1601)*365 + 363 + 72)
// where 363 is the number of days in 1899 before 12/30, and 72 is the number
// of leap year days between 1601 and 1899 (exluding 1700 and 1800):
// (1899-1601)/4 - 2
constexpr uint64_t kWinRTToVariantDays = 109205;

// Offset in days of the VARIANT DATE epoch (1899-12-30 00:00:00 UTC) from the
// JS Date UNIX epoch (1970-01-01 00:00:00 UTC). This value is derived from the
// following:
// ((1970-1900)*365 + 2 + 17)
// where 2 is the number of days from 12/30/1899 to 1/1/1900, and 16 is the number
// of leap year days between 1899 and 1970 (excluding 1900):
// (1970-1899)/4 - 1
constexpr uint64_t kVariantToJsDays = 25569;

constexpr uint64_t kWinRTToJsDays = 134774; // kWinRTToVariantDays + kVariantToJsDays;

// JS time is measured in millisecond ticks.
// Each day is 24 * 60 * 60 * 1000 = 86400000 ticks
constexpr uint64_t kMillisecondsPerDay = 86400000;

// WinRT uses FileTime which is measured in 100-nanosecond ticks.
// Millisecond = 10^-3, HundredNanosecond = 10^-7, diff = 10^4
constexpr uint64_t kHundredNanosecondsPerMillisecond = 10000;

constexpr uint64_t kHundredNanosecondsPerDay =
    kMillisecondsPerDay * kHundredNanosecondsPerMillisecond;

// Converts a DATE (double, fractional days since DATE epoch 12/30/1899)
// to JS time (uint64_t, count of milliseconds since UNIX epoch 1/1/1970)
inline int64_t VariantTimeToJsTime(const DATE& date)
{
    // DATEs are weird - the day is signed and represents +/- that many days from
    // the epoch. But the fractional part of the day is unsigned. To fix for this
    // we separate the days from the fractional day part, then add back the
    // absolute value of the fractional day to get a true unsigned day count.
    // Example: -10.2 DATE becomes -10 + abs(-0.2) = -9.8
    double countDay;
    double fractionalDay = std::modf(date, &countDay);
    double daysSinceVariantEpoch = countDay + std::abs(fractionalDay);

    // Move from Variant DATE epoch to JS UNIX epoch
    double daysSinceJsEpoch = daysSinceVariantEpoch - kVariantToJsDays;
    // Then we convert to milliseconds
    int64_t millisecondsSinceJsEpoch = std::llround(daysSinceJsEpoch * kMillisecondsPerDay);
    return millisecondsSinceJsEpoch;
}

inline double JsTimeToWinRTTime(const double& time)
{
    // JS Time is in milliseconds since UNIX epoch. First, move to the Windows epoch.
    double millisecondsSinceWindowsEpoch = time + (kWinRTToJsDays * kMillisecondsPerDay);

    // Then convert to 100-nanoseconds
    return millisecondsSinceWindowsEpoch * kHundredNanosecondsPerMillisecond;
}

inline DATE WinRTTimeToVariantTime(const int64_t& time)
{
    // Convert to Days
    double daysSinceWindowsEpoch = static_cast<double>(time) / kHundredNanosecondsPerDay;
    // Convert from Windows epoch days to DATE epoch days
    double daysSinceVariantEpoch = daysSinceWindowsEpoch - kWinRTToVariantDays;

    // Once again, DATEs are weird with the signed day, but always positive
    // fractional day piece. To transform a normal days amount into DATE we need
    // convert the fractional piece to be 1+fractional and change the sign if the
    // value is negative.
    double countDay;
    double fractionalDay = std::modf(daysSinceVariantEpoch, &countDay);
    if (fractionalDay == 0.0)
    {
        // There's an interesting edge-case when there is no fractional day part and
        // a negative day count. For the function below, as
        // fractional day goes to 0, the resulting difference goes to 2. For example
        // -1.00001 becomes -2.99999, a difference of 1.99998 (almost 2). Normally
        // this is what we want, but for 0 fractional day we don't want to change,
        // so we return the count directly.
        return countDay;
    }
    return (countDay - 1) + copysign(1 + fractionalDay, daysSinceVariantEpoch);
}
} // namespace wv2winrt_impl

