// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#pragma once
#include <windows.h>

namespace wv2winrt_impl
{
struct UniqueVariant : public VARIANT
{
    UniqueVariant();
    UniqueVariant(const VARIANT&);
    ~UniqueVariant();

    VARIANT* addressof();
    void reset();
    void reset(const VARIANT&);
    VARIANT* reset_and_addressof();

    VARIANT release();

private:
    void init();
    void close();

    UniqueVariant(const UniqueVariant&) = delete;
    UniqueVariant& operator=(const UniqueVariant&) = delete;
};
} // namespace wv2winrt_impl
