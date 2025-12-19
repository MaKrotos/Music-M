// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#include "pch.h"
// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#include "wv2winrt/uniquevariant.h"

wv2winrt_impl::UniqueVariant::UniqueVariant()
{
    init();
}

wv2winrt_impl::UniqueVariant::UniqueVariant(const VARIANT& other) : VARIANT(other)
{
}

wv2winrt_impl::UniqueVariant::~UniqueVariant()
{
    close();
}

VARIANT* wv2winrt_impl::UniqueVariant::addressof()
{
    return this;
}

void wv2winrt_impl::UniqueVariant::reset()
{
    close();
    init();
}

void wv2winrt_impl::UniqueVariant::reset(const VARIANT& other)
{
    close();
    VARIANT::operator=(other);
}

VARIANT* wv2winrt_impl::UniqueVariant::reset_and_addressof()
{
    reset();
    return addressof();
}

VARIANT wv2winrt_impl::UniqueVariant::release()
{
    VARIANT result(*this);
    // Note, we're explicitly not calling close here.
    // The whole point of release is we're leaking
    // the struct to the caller.
    init();
    return result;
}

void wv2winrt_impl::UniqueVariant::init()
{
    ::VariantInit(this);
}

void wv2winrt_impl::UniqueVariant::close()
{
    ::VariantClear(this);
}

