// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#pragma once

// Must be included before any cppwinrt headers
#include <wv2winrt/base.h>

#include <winrt/Microsoft.Web.WebView2.Core.h>

namespace wv2winrt_impl
{

typedef HRESULT CreateFn(
    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter,
    IDispatch** dispatch);

typedef winrt::com_ptr<IDispatch> CreateFromInstanceFn(
    IInspectable* objectAsABI,
    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter);

struct GlobalEntry
{
    LPCWSTR name;
    CreateFn* createFunction;
};

struct InstanceConstructibleEntry
{
    LPCWSTR name;
    bool isBoxedStruct;
    CreateFromInstanceFn* createFunction;

    bool operator<(const InstanceConstructibleEntry& rhs) const
    {
        return wcscmp(name, rhs.name) == -1;
    }
};

extern const GlobalEntry s_globalEntries[];
extern const size_t s_globalEntriesCount;
extern const InstanceConstructibleEntry s_instanceConstructibleEntries[];
extern const size_t s_instanceConstructibleEntriesCount;

} // namespace wv2winrt_impl

