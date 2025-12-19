// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#pragma once
#include <windows.h>

#include <winrt/Windows.Foundation.h>
namespace wv2winrt_impl
{

struct __declspec(uuid("32A1E429-C362-48B3-832D-4A895810C573"))
    ICoreWebView2PrivateDispatchAsyncFinishedHandler : ::IUnknown
{
    virtual HRESULT STDMETHODCALLTYPE Invoke(HRESULT errorCode, VARIANT* result) = 0;
};

struct __declspec(uuid("79EDD0B3-1E40-4A58-B771-FE8B4F380DF9"))
    ICoreWebView2PrivateDispatchAsyncResult : ::IUnknown
{
    // Add an IDispatch that will be invoked after async completion. If provided before
    // async completion it will be invoked later after async completion. If provided after
    // async completion it will be invoked immediately.
    virtual HRESULT STDMETHODCALLTYPE SetCompletedHandler(
        ICoreWebView2PrivateDispatchAsyncFinishedHandler* completedCallback) = 0;
};

// Creates an object that implements ICoreWebView2PrivateDispatchAsyncResult to allow callers to
// observe async completion. It also implements IDispatch and can be Invoked to set the result
// of async completion.
HRESULT CreateDispatchAsyncResult(ICoreWebView2PrivateDispatchAsyncResult** asyncResult);

struct __declspec(uuid("82fe62e1-a009-427c-8d1c-59b0a377b73d"))
    ICoreWebView2PrivateDispatchAsyncResult2 : ::IUnknown
{
    virtual HRESULT STDMETHODCALLTYPE Cancel() = 0;
};

struct __declspec(uuid("8df7da06-d243-4ef8-b927-fe346d2e7ff6"))
    ICoreWebView2PrivateDispatchAsyncInfo : ::IUnknown
{
    virtual HRESULT STDMETHODCALLTYPE
    SetAsyncInfo(winrt::Windows::Foundation::IAsyncInfo asyncInfo) = 0;
};

} // namespace wv2winrt_impl

