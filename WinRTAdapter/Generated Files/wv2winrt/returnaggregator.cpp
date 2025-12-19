// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#include "pch.h"
// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#include "wv2winrt/returnaggregator.h"

wv2winrt_impl::ReturnAggregator::ReturnAggregator(
    const size_t outParamCount, const bool hasReturnValue)
    : m_parameterValues(outParamCount + (hasReturnValue ? 1 : 0))
{
}

VARIANT* wv2winrt_impl::ReturnAggregator::AddParameter(LPCWSTR name)
{
    const size_t idx = m_parameterNameToIdxMap.size();
    m_parameterNameToIdxMap.emplace(name, idx);
    assert(idx < m_parameterValues.size());
    return m_parameterValues[idx].reset_and_addressof();
}

void wv2winrt_impl::ReturnAggregator::SetReturnValue(const VARIANT& value)
{
    VariantCopy(AddParameter(L"value"), &value);
}

// IDispatch methods
HRESULT wv2winrt_impl::ReturnAggregator::GetIDsOfNames(
    REFIID riid, LPOLESTR* names, unsigned int namesCount, LCID localeId, DISPID* dispId)
{
    HRESULT hr = DispatchBase::GetIDsOfNames(riid, names, namesCount, localeId, dispId);
    if (SUCCEEDED(hr))
    {
        hr = DISP_E_MEMBERNOTFOUND;
        auto nameToIdxItr = m_parameterNameToIdxMap.find(names[0]);
        if (nameToIdxItr != m_parameterNameToIdxMap.end())
        {
            *dispId = static_cast<DISPID>(nameToIdxItr->second + 1);
            hr = S_OK;
        }
    }
    return hr;
}

HRESULT wv2winrt_impl::ReturnAggregator::Invoke(
    DISPID dispId, REFIID riid, LCID localeId, WORD flags, DISPPARAMS* dispParams,
    VARIANT* result, EXCEPINFO* excepInfo, UINT* argErr)
{
    HRESULT hr = DispatchBase::Invoke(
        dispId, riid, localeId, flags, dispParams, result, excepInfo, argErr);

    if (SUCCEEDED(hr))
    {
        hr = DISP_E_MEMBERNOTFOUND;
        if (flags == DISPATCH_PROPERTYGET)
        {
            size_t entryIdx = dispId - 1;
            if (entryIdx < m_parameterValues.size())
            {
                hr = VariantCopy(result, m_parameterValues[entryIdx].addressof());
            }
        }
    }
    return hr;
}

HRESULT wv2winrt_impl::ReturnAggregator::AddOutArrayParameterInfoEntry(LPWSTR name, int index)
{
    m_OutArrayParameterNames.push_back(name);
    m_OutArrayParameterIndexes.push_back(index);
    return S_OK;
}

HRESULT wv2winrt_impl::ReturnAggregator::GetOutArrayParameterInfo(
    int** indexes, LPWSTR** names, size_t* length)
{
    if (m_OutArrayParameterNames.size() == 0 || m_OutArrayParameterIndexes.size() == 0)
    {
        return E_NOTIMPL;
    }
    *names = m_OutArrayParameterNames.data();
    *indexes = m_OutArrayParameterIndexes.data();
    *length = m_OutArrayParameterNames.size();
    return S_OK;
}
