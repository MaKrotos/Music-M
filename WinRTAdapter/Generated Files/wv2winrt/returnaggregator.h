// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#pragma once
#include <map>
#include <windows.h>

#include "wv2winrt/dispatchbase.h"
#include "wv2winrt/uniquevariant.h"

namespace wv2winrt_impl
{
struct ReturnAggregator : winrt::implements<ReturnAggregator, DispatchBase>
{
    ReturnAggregator(const size_t outParamCount, const bool hasReturnValue);

    // Add a named out parameter. Uses this peculiar signature so that it can
    // be called easily with an out parameter that will write to the variant
    // value.
    VARIANT* AddParameter(LPCWSTR name);
    void SetReturnValue(const VARIANT& value);

    // IDispatch methods
    HRESULT STDMETHODCALLTYPE GetIDsOfNames(
        REFIID riid, LPOLESTR* names, unsigned int namesCount, LCID localeId,
        DISPID* dispId) override;

    HRESULT STDMETHODCALLTYPE Invoke(
        DISPID dispId, REFIID riid, LCID localeId, WORD flags, DISPPARAMS* dispParams,
        VARIANT* result, EXCEPINFO* excepInfo, UINT* argErr) override;

    // ICoreWebView2PrivateDispatchContainer4 methods
    HRESULT STDMETHODCALLTYPE
    GetOutArrayParameterInfo(int** indexes, LPWSTR** names, size_t* length) override;

    HRESULT STDMETHODCALLTYPE AddOutArrayParameterInfoEntry(LPWSTR, int);

private:
    std::map<std::wstring, size_t> m_parameterNameToIdxMap;
    std::vector<UniqueVariant> m_parameterValues;
    std::vector<LPWSTR> m_OutArrayParameterNames;
    std::vector<int> m_OutArrayParameterIndexes;
};
} // namespace wv2winrt_impl

