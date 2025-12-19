// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#pragma once

// Must be included before any cppwinrt headers
#include <wv2winrt/base.h>

#include <winrt/Microsoft.Web.WebView2.Core.h>

namespace wv2winrt_impl
{

HRESULT STDMETHODCALLTYPE CreateDispatchFromName(
    LPCWSTR name,
    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter,
    IDispatch** dispatch);
HRESULT STDMETHODCALLTYPE CreateDispatchFromInspectable(
    IInspectable* object,
    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter,
    IDispatch** dispatch, bool* is_cacheable);

} // namespace wv2winrt_impl

