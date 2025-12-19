// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#include "pch.h"
// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#include "wv2winrt/main.h"
#include "wv2winrt/globals.h"

namespace wv2winrt_impl
{

const InstanceConstructibleEntry* LookupInstanceConstructibleEntry(LPCWSTR query)
{
    const InstanceConstructibleEntry* last =
        &(s_instanceConstructibleEntries[s_instanceConstructibleEntriesCount]);
    InstanceConstructibleEntry queryEntry{query, false, nullptr};
    const InstanceConstructibleEntry* it =
        std::lower_bound(s_instanceConstructibleEntries, last, queryEntry);
    return (it == last || queryEntry < *it) ? nullptr : &(*it);
}

HRESULT STDMETHODCALLTYPE CreateDispatchFromName(
    LPCWSTR name,
    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter,
    IDispatch** dispatch)
{
    for (size_t globalEntriesIdx = 0; globalEntriesIdx < s_globalEntriesCount;
         ++globalEntriesIdx)
    {
        if (wcscmp(s_globalEntries[globalEntriesIdx].name, name) == 0)
        {
            return s_globalEntries[globalEntriesIdx].createFunction(dispatchAdapter, dispatch);
        }
    }
    return DISP_E_MEMBERNOTFOUND;
}

HRESULT STDMETHODCALLTYPE CreateDispatchFromInspectable(
    IInspectable* objectAsABI,
    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter,
    IDispatch** result, bool* is_cacheable)
{
    if (!objectAsABI)
    {
        return E_POINTER;
    }
    *result = nullptr;
    HSTRING classNameAsHSTRING = nullptr;
    HRESULT hr = objectAsABI->GetRuntimeClassName(&classNameAsHSTRING);
    if (SUCCEEDED(hr))
    {
        hr = DISP_E_MEMBERNOTFOUND;
        LPCWSTR classNameAsLPCWSTR = WindowsGetStringRawBuffer(classNameAsHSTRING, nullptr);
        const InstanceConstructibleEntry* entry =
            LookupInstanceConstructibleEntry(classNameAsLPCWSTR);
        if (entry)
        {
            auto obj = entry->createFunction(objectAsABI, dispatchAdapter);
            *result = obj.detach();
            *is_cacheable = !(entry->isBoxedStruct);
            hr = S_OK;
        }
    }
    WindowsDeleteString(classNameAsHSTRING);

    return hr;
}

} // namespace wv2winrt_impl

