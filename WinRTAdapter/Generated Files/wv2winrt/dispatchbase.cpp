// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#include "pch.h"
// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#include "wv2winrt/dispatchbase.h"
#include "wv2winrt/uniquevariant.h"

namespace wv2winrt_impl
{
HRESULT DispatchBase::GetTypeInfoCount(UINT* /* typeInfoCount */)
{
    return E_NOTIMPL;
}

HRESULT DispatchBase::GetTypeInfo(
    UINT /* typeInfoCount */, LCID /* localeId */, ITypeInfo** /* typeInfo */)
{
    return E_NOTIMPL;
}

HRESULT DispatchBase::GetIDsOfNames(
    REFIID riid, LPOLESTR*, unsigned int namesCount, LCID localeId, DISPID*)
{
    if (riid != IID_NULL || localeId != LOCALE_USER_DEFAULT || namesCount != 1)
    {
        return E_INVALIDARG;
    }

    return S_OK;
}

HRESULT DispatchBase::Invoke(
    DISPID, REFIID riid, LCID localeId, WORD flags, DISPPARAMS* dispParams, VARIANT*,
    EXCEPINFO*, UINT*)
{
    if (riid != IID_NULL || localeId != LOCALE_USER_DEFAULT)
    {
        return E_INVALIDARG;
    }
    if (flags == DISPATCH_PROPERTYPUT)
    {
        if (dispParams->cNamedArgs != 1U || dispParams->rgdispidNamedArgs == nullptr)
        {
            return E_INVALIDARG;
        }
    }
    else
    {
        if (dispParams->cNamedArgs != 0U || dispParams->rgdispidNamedArgs != nullptr)
        {
            return E_INVALIDARG;
        }
    }

    return S_OK;
}

// static
bool DispatchBase::LookupProperty(
    DispatchProperties properties, LPOLESTR const& query, size_t& idx, size_t count)
{
    DispatchProperties const end = &(properties[count]);
    DispatchProperties entry = std::lower_bound(
        properties, end, query,
        [](const wchar_t* const lhs, LPOLESTR const rhs) { return wcscmp(lhs, rhs) == -1; });
    if (entry == end || wcscmp(query, *entry) == -1)
    {
        return false;
    }
    idx = std::distance(properties, entry);
    return true;
}
} // namespace wv2winrt_impl

