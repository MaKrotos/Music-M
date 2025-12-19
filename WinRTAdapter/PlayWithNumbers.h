#pragma once

#include "pch.h"

namespace winrt::NativeCode::implementation
{
    struct PlayWithNumbers : PlayWithNumbersT<PlayWithNumbers>
    {
        PlayWithNumbers() = default;

        int32_t Add(int32_t num1, int32_t num2);
    };
}

namespace winrt::NativeCode::factory_implementation
{
    struct PlayWithNumbers : PlayWithNumbersT<PlayWithNumbers, implementation::PlayWithNumbers>
    {
    };
}