// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#pragma once
#include <basetsd.h>
#include <windows.h>

namespace wv2winrt_impl
{

// These should be in sync with the ones in WebView2Private.idl
enum DispatchContainerScriptBehavior
{
    DispatchContainerScriptBehaviorNone = 0,
    DispatchContainerScriptBehaviorVector,
    DispatchContainerScriptBehaviorMap
};

struct __declspec(uuid("2E324595-31A2-4797-9F23-A657B22A1E4B"))
    ICoreWebView2PrivateDispatchContainer : ::IUnknown
{
    virtual HRESULT STDMETHODCALLTYPE GetInnerObject(IUnknown** object) = 0;
    virtual HRESULT STDMETHODCALLTYPE GetBasisVariant(VARIANT* basis) = 0;
    virtual HRESULT STDMETHODCALLTYPE
    GetPropertyNames(wchar_t*** names, size_t* namesLength) = 0;
    virtual HRESULT STDMETHODCALLTYPE GetScriptBehavior(DWORD* behaviorIdx) = 0;
};

struct __declspec(uuid("349f926b-227f-40ef-a359-b941253ed0d3"))
    ICoreWebView2PrivateDispatchContainer2 : ::IUnknown
{
    virtual HRESULT STDMETHODCALLTYPE
    IsPropertyCacheable(DISPID propertyId, BOOL* isCacheable) = 0;
};

struct __declspec(uuid("6251dc5f-b565-43a6-83e4-39cd5e9d9729"))
    ICoreWebView2PrivateDispatchContainer3 : ::IUnknown
{
    virtual HRESULT STDMETHODCALLTYPE
    GetPropertiesPrecacheability(BOOL** boolsOut, size_t* propertiesLengthOut) = 0;
};

struct __declspec(uuid("e4970e01-8008-4092-9c5c-28d147394f41"))
ICoreWebView2PrivateDispatchContainer4 : ::IUnknown
{
    virtual HRESULT STDMETHODCALLTYPE
    GetOutArrayParameterInfo(int** indexes, LPWSTR** names, size_t* length) = 0;
};

} // namespace wv2winrt_impl

