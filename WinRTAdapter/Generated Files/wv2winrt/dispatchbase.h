// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#pragma once
#include <windows.h>

// Must be included before any cppwinrt headers
#include <wv2winrt/base.h>

#include <winrt/Microsoft.Web.WebView2.Core.h>

#include "wv2winrt/converter.h"
#include "wv2winrt/dispatchcontainer.h"

namespace wv2winrt_impl
{
typedef const wchar_t* const* const DispatchProperties;

struct DispatchBase
    : winrt::implements<
          DispatchBase, IDispatch, winrt::Windows::Foundation::IInspectable,
          ICoreWebView2PrivateDispatchContainer, ICoreWebView2PrivateDispatchContainer2,
          ICoreWebView2PrivateDispatchContainer3, ICoreWebView2PrivateDispatchContainer4>
{
    // IDispatch methods
    HRESULT STDMETHODCALLTYPE GetTypeInfoCount(UINT* typeInfoCount) override;
    HRESULT STDMETHODCALLTYPE
    GetTypeInfo(UINT typeInfoCount, LCID localeId, ITypeInfo** typeInfo) override;
    HRESULT STDMETHODCALLTYPE GetIDsOfNames(
        REFIID riid, LPOLESTR* names, unsigned int namesCount, LCID localeId,
        DISPID* dispId) override;
    HRESULT STDMETHODCALLTYPE Invoke(
        DISPID dispId, REFIID riid, LCID localeId, WORD flags, DISPPARAMS* dispParams,
        VARIANT* result, EXCEPINFO* excepInfo, UINT* argErr) override;

    // ICoreWebView2PrivateDispatchContainer methods
    HRESULT STDMETHODCALLTYPE GetInnerObject(IUnknown**) override
    {
        return E_NOTIMPL;
    };
    HRESULT STDMETHODCALLTYPE GetBasisVariant(VARIANT*) override
    {
        return E_NOTIMPL;
    };
    HRESULT STDMETHODCALLTYPE GetPropertyNames(wchar_t***, size_t*) override
    {
        return E_NOTIMPL;
    };
    HRESULT STDMETHODCALLTYPE GetScriptBehavior(DWORD* behavior) override
    {
        *behavior = DispatchContainerScriptBehaviorNone;
        return S_OK;
    };

    // ICoreWebView2PrivateDispatchContainer2 methods
    HRESULT STDMETHODCALLTYPE IsPropertyCacheable(DISPID, BOOL* isCacheable) override
    {
        *isCacheable = FALSE;
        return S_OK;
    }

    // ICoreWebView2PrivateDispatchContainer3 methods
    HRESULT STDMETHODCALLTYPE GetPropertiesPrecacheability(BOOL**, size_t*) override
    {
        return E_NOTIMPL;
    };

    // ICoreWebView2PrivateDispatchContainer4 methods
    HRESULT STDMETHODCALLTYPE GetOutArrayParameterInfo(int**, LPWSTR**, size_t*) override
    {
        return E_NOTIMPL;
    }

    __declspec(noinline) static bool LookupProperty(
        DispatchProperties properties, LPOLESTR const& query, size_t& idx, size_t count);
};

} // namespace wv2winrt_impl

