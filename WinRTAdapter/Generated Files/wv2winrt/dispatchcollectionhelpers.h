// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#pragma once
#include <windows.h>

// Must be included before any cppwinrt headers
#include <wv2winrt/base.h>

#include <winrt/Microsoft.Web.WebView2.Core.h>

#include "wv2winrt/converter.h"
#include "wv2winrt/returnaggregator.h"

namespace wv2winrt_impl
{
// This is a helper class rather than a base class because GetIDsOfNames and Invoke
// must only be called after the more derived type has a chance to handle GetIDsOfNames
// and Invoke. So that type can explicitly use the helper class.
template <typename TVector, bool TIsWritable, typename TVectorValue> struct DispatchVectorHelper
{
    DispatchVectorHelper(
        const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
            dispatchAdapter,
        DISPID idOffset)
        : m_dispatchAdapter(dispatchAdapter), m_idOffset(idOffset)
    {
    }

    HRESULT STDMETHODCALLTYPE GetIDsOfNames(LPOLESTR* names, DISPID* dispId)
    {
        HRESULT hr = S_OK;
        try
        {
            if (wcscmp(L"length", names[0]) == 0)
            {
                *dispId = m_idOffset;
            }
            else
            {
                size_t nameAsIdx = std::stol(names[0]);
                const DISPID id =
                    static_cast<DISPID>(m_idOffset + s_arrayIndiciesOffset + nameAsIdx);
                *dispId = id;
            }
        }
        catch (const std::invalid_argument&)
        {
            hr = DISP_E_MEMBERNOTFOUND;
        }

        return hr;
    }

    HRESULT STDMETHODCALLTYPE Invoke(
        const TVector& vector, DISPID dispId, WORD flags, DISPPARAMS* dispParams,
        VARIANT* result)
    {
        HRESULT hr = DISP_E_MEMBERNOTFOUND;
        const uint32_t originalVectorSize = vector.Size();
        const uint32_t namesIdx = dispId - m_idOffset - s_arrayIndiciesOffset;

        if (dispId == m_idOffset)
        {
            // length property
            try
            {
                if (flags == DISPATCH_PROPERTYGET)
                {
                    *result =
                        Converter<decltype(originalVectorSize), VARIANT>(m_dispatchAdapter)
                            .Convert(originalVectorSize);
                    hr = S_OK;
                }
                else if (flags == DISPATCH_PROPERTYPUT)
                {
                    hr = E_INVALIDARG;

                    if constexpr (TIsWritable)
                    {
                        int targetVectorSize =
                            Converter<VARIANT, decltype(targetVectorSize)>(m_dispatchAdapter)
                                .Convert(*(dispParams->rgvarg));
                        if (targetVectorSize >= 0)
                        {
                            EnsureSize(vector, static_cast<uint32_t>(targetVectorSize), false);
                            hr = S_OK;
                        }
                    }
                }
            }
            catch (winrt::hresult_error)
            {
                hr = winrt::to_hresult();
            }
            catch (...)
            {
                hr = E_UNEXPECTED;
            }
        }
        else
        {
            try
            {
                // If we are on an array, we don't know if we are getting an
                // element in the array or if we are getting a method, which
                // would look like we are extending off the end of the array.
                if (flags == DISPATCH_PROPERTYGET && namesIdx < originalVectorSize)
                {
                    auto resultAsWinRT = vector.GetAt(namesIdx);

                    *result = Converter<decltype(resultAsWinRT), VARIANT>(m_dispatchAdapter)
                                  .Convert(resultAsWinRT);
                    hr = S_OK;
                }
                else if (flags == DISPATCH_PROPERTYPUT)
                {
                    if constexpr (TIsWritable)
                    {
                        // Setting a value larger than the size of a JS array
                        // increases the size of the JS array to fit.
                        EnsureSize(vector, namesIdx + 1);
                        vector.SetAt(
                            namesIdx, Converter<VARIANT, TVectorValue>(m_dispatchAdapter)
                                          .Convert(*(dispParams->rgvarg)));
                        hr = S_OK;
                    }
                }
            }
            catch (winrt::hresult_error)
            {
                hr = winrt::to_hresult();
            }
            catch (...)
            {
                hr = E_UNEXPECTED;
            }
        }

        return hr;
    }

private:
    void EnsureSize(const TVector& vector, uint32_t newVectorSize, bool atLeast = true)
    {
        const uint32_t originalVectorSize = vector.Size();

        if (newVectorSize > originalVectorSize)
        {
            uint32_t currentVectorSize = originalVectorSize;
            while (newVectorSize > currentVectorSize++)
            {
                vector.Append(GetDefaultValue<TVectorValue>());
            }
        }
        else if (newVectorSize < originalVectorSize && !atLeast)
        {
            uint32_t currentVectorSize = originalVectorSize;
            while (newVectorSize < currentVectorSize--)
            {
                vector.RemoveAtEnd();
            }
        }
    }

    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
    DISPID m_idOffset = 0;
    static const size_t s_arrayIndiciesOffset = 1;
};

// This is a helper class rather than a base class because GetIDsOfNames and Invoke
// must only be called after the more derived type has a chance to handle GetIDsOfNames
// and Invoke. So that type can explicitly use the helper class.
template <typename TInner, bool TIsWritable, typename TMapValue> struct DispatchMapHelper
{
    DispatchMapHelper(
        const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
            dispatchAdapter,
        DISPID idOffset)
        : m_dispatchAdapter(dispatchAdapter), m_idOffset(idOffset)
    {
    }

    HRESULT STDMETHODCALLTYPE GetIDsOfNames(LPOLESTR* names, DISPID* dispId)
    {
        HRESULT hr = S_OK;

        if (wcscmp(names[0], L"getAllKeyNames") == 0)
        {
            *dispId = m_idOffset;
        }
        else
        {
            auto itr = m_nameToIdMap.find(names[0]);
            if (itr != m_nameToIdMap.end())
            {
                *dispId = itr->second;
            }
            else
            {
                const size_t idx = m_names.size();
                const DISPID id = static_cast<DISPID>((m_idOffset + s_knownIds) + idx);
                std::wstring nameAsWstring = names[0];
                m_names.push_back(nameAsWstring);
                m_nameToIdMap.try_emplace(nameAsWstring, id);
                *dispId = id;
            }
        }

        return hr;
    }

    const std::vector<winrt::hstring> GetKeyNames(const TInner& map)
    {
        std::vector<winrt::hstring> keys(map.Size());
        auto itr = map.First();
        size_t idx = 0;

        if (itr.HasCurrent())
        {
            do
            {
                keys[idx++] = itr.Current().Key();
            } while (itr.MoveNext());
        }
        return keys;
    }

    HRESULT STDMETHODCALLTYPE Invoke(
        const TInner& inner, DISPID dispId, WORD flags, DISPPARAMS* dispParams, VARIANT* result)
    {
        HRESULT hr = DISP_E_MEMBERNOTFOUND;

        if (dispId < m_idOffset + s_knownIds)
        {
            if (dispId == m_idOffset)
            {
                try
                {
                    if (flags == DISPATCH_METHOD)
                    {
                        auto returnAggregator =
                            winrt::make_self<wv2winrt_impl::ReturnAggregator>(2, false);
                        *(returnAggregator->AddParameter(L"cacheable")) =
                            Converter<bool, VARIANT>(m_dispatchAdapter).Convert(!TIsWritable);
                        std::vector<winrt::hstring> keyNamesAsVectorHstring =
                            GetKeyNames(inner);
                        winrt::array_view<winrt::hstring> keyNamesAsArrayViewHstring(
                            keyNamesAsVectorHstring);
                        *(returnAggregator->AddParameter(L"keyNames")) =
                            Converter<winrt::array_view<winrt::hstring>, VARIANT>(
                                m_dispatchAdapter)
                                .Convert(keyNamesAsArrayViewHstring);
                        result->vt = VT_DISPATCH;
                        returnAggregator.as<::IDispatch>().copy_to(&result->pdispVal);
                        hr = S_OK;
                    }
                }
                catch (winrt::hresult_error)
                {
                    hr = winrt::to_hresult();
                }
                catch (...)
                {
                    hr = E_UNEXPECTED;
                }
            }
        }
        else
        {
            const size_t namesIdx = dispId - (m_idOffset + s_knownIds);
            if (namesIdx < m_names.size())
            {
                try
                {
                    const std::wstring& propertyName = m_names[namesIdx];
                    if (flags == DISPATCH_PROPERTYGET)
                    {
                        if (inner.HasKey(propertyName))
                        {
                            auto resultAsWinRT = inner.Lookup(propertyName);
                            *result =
                                Converter<decltype(resultAsWinRT), VARIANT>(m_dispatchAdapter)
                                    .Convert(resultAsWinRT);
                        }
                        else
                        {
                            *result = {VT_NULL};
                        }
                        hr = S_OK;
                    }
                    else if (flags == DISPATCH_PROPERTYPUT)
                    {
                        if constexpr (TIsWritable)
                        {
                            inner.Insert(
                                propertyName, Converter<VARIANT, TMapValue>(m_dispatchAdapter)
                                                  .Convert(*(dispParams->rgvarg)));
                            hr = S_OK;
                        }
                    }
                }
                catch (winrt::hresult_error)
                {
                    hr = winrt::to_hresult();
                }
                catch (...)
                {
                    hr = E_UNEXPECTED;
                }
            }
        }

        return hr;
    }

private:
    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
    DISPID m_idOffset = 0;
    static const size_t s_knownIds = 1;
    std::map<std::wstring, DISPID> m_nameToIdMap;
    std::vector<std::wstring> m_names;
};

} // namespace wv2winrt_impl

