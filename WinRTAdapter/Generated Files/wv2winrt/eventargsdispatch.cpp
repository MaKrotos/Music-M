// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#include "pch.h"
// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#include "wv2winrt/eventargsdispatch.h"
#include "wv2winrt/dispatchbase.h"
#include "wv2winrt/uniquevariant.h"

namespace wv2winrt_impl
{
// Class that merges event args and source winrt event objects into one
// aggregate object that has a target and detail properties. This merges
// the unmodified event args with custom properties for target and detail.
struct AggregateEventArgs : winrt::implements<AggregateEventArgs, DispatchBase>
{
    AggregateEventArgs(
        LPCWSTR eventName, IDispatch* source, VARIANT* unmodifiedDelegateParamsAsVARIANT,
        const ULONG unmodifiedDelegateParamsAsVARIANTCount)
    {
        m_typeString.vt = VT_BSTR;
        m_typeString.bstrVal = SysAllocString(eventName);

        m_source.copy_from(source);

        PROPVARIANT vector_propvariant;
        vector_propvariant.vt = VT_VECTOR | VT_VARIANT;
        vector_propvariant.capropvar.cElems = unmodifiedDelegateParamsAsVARIANTCount;
        vector_propvariant.capropvar.pElems = reinterpret_cast<PROPVARIANT*>(
            CoTaskMemAlloc(sizeof(PROPVARIANT) * unmodifiedDelegateParamsAsVARIANTCount));

        for (uint32_t idx = 0; idx < unmodifiedDelegateParamsAsVARIANTCount; ++idx)
        {
            UniqueVariant unmodifiedParamAsVARIANT;

            if (idx == 0) // Handling the event args
            {
                VariantCopy(
                    m_unmodifiedEventArgsAsVARIANT.reset_and_addressof(),
                    &unmodifiedDelegateParamsAsVARIANT[idx]);

                if (m_unmodifiedEventArgsAsVARIANT.vt == VT_DISPATCH &&
                    m_unmodifiedEventArgsAsVARIANT.pdispVal != nullptr)
                {
                    m_unmodifiedEventArgsAsIDispatch.copy_from(
                        m_unmodifiedEventArgsAsVARIANT.pdispVal);
                    m_detailValue.vt = VT_DISPATCH;
                    m_unmodifiedEventArgsAsIDispatch.copy_to(&m_detailValue.pdispVal);
                }
                else if (
                    m_unmodifiedEventArgsAsVARIANT.vt == VT_DISPATCH &&
                    m_unmodifiedEventArgsAsVARIANT.pdispVal == nullptr)
                {
                    m_detailValue.pdispVal = nullptr;
                    m_detailValue.vt = VT_NULL;
                }
                else
                {
                    VariantCopy(
                        m_detailValue.reset_and_addressof(),
                        m_unmodifiedEventArgsAsVARIANT.addressof());
                }
                vector_propvariant.capropvar.pElems[idx] =
                    *(reinterpret_cast<PROPVARIANT*>(m_detailValue.addressof()));
            }
            else
            {
                VariantCopy(
                    unmodifiedParamAsVARIANT.reset_and_addressof(),
                    &unmodifiedDelegateParamsAsVARIANT[idx]);
                vector_propvariant.capropvar.pElems[idx] =
                    *(reinterpret_cast<PROPVARIANT*>(unmodifiedParamAsVARIANT.addressof()));
            }
        }

        static_assert(sizeof(PROPVARIANT) == sizeof(VARIANT));
        m_detailVector.reset(*reinterpret_cast<VARIANT*>(&vector_propvariant));
    }

    // IDispatch methods
    // GetIDsOfNames knows about hardcoded target and detail properties. Other names
    // fall through to the unmodified event args. To avoid ID collisions the unmodified
    // IDs are offset.
    HRESULT STDMETHODCALLTYPE GetIDsOfNames(
        REFIID riid, LPOLESTR* names, unsigned int namesCount, LCID localeId,
        DISPID* dispId) override
    {
        HRESULT hr = DispatchBase::GetIDsOfNames(riid, names, namesCount, localeId, dispId);
        if (SUCCEEDED(hr))
        {
            hr = DISP_E_MEMBERNOTFOUND;
            if (wcscmp(names[0], L"target") == 0)
            {
                *dispId = s_targetId;
                hr = S_OK;
            }
            else if (wcscmp(names[0], L"detail") == 0)
            {
                *dispId = s_detailId;
                hr = S_OK;
            }
            else if (wcscmp(names[0], L"type") == 0)
            {
                *dispId = s_typeId;
                hr = S_OK;
            }
            else if (m_unmodifiedEventArgsAsIDispatch != nullptr)
            {
                // If its not one of our known names that we're adding to the event args, then
                // lets ask the actual event args.
                hr = m_unmodifiedEventArgsAsIDispatch->GetIDsOfNames(
                    riid, names, namesCount, localeId, dispId);
                if (SUCCEEDED(hr))
                {
                    // 0 is a special DISPID that refers to the current object
                    // Our names are injected after 0 and before all other DISPIDs
                    // so if its not 0, then we move the ID down to make room.
                    if (*dispId > 0)
                    {
                        *dispId += s_dispIdOffset;
                    }
                }
            }
        }
        return hr;
    }

    HRESULT STDMETHODCALLTYPE Invoke(
        DISPID dispId, REFIID riid, LCID localeId, WORD flags, DISPPARAMS* dispParams,
        VARIANT* result, EXCEPINFO* excepInfo, UINT* argErr) override
    {
        HRESULT hr = DispatchBase::Invoke(
            dispId, riid, localeId, flags, dispParams, result, excepInfo, argErr);
        bool modifiedId = false;

        if (SUCCEEDED(hr))
        {
            hr = DISP_E_MEMBERNOTFOUND;
            if (flags == DISPATCH_PROPERTYGET)
            {
                switch (dispId)
                {
                case s_targetId:
                {
                    if (m_source != nullptr)
                    {

                        result->vt = VT_DISPATCH;
                        m_source.copy_to(&result->pdispVal);
                    }
                    else
                    {
                        result->vt = VT_NULL;
                    }
                    modifiedId = true;
                    hr = S_OK;
                    break;
                }

                case s_detailId:
                {
                    *result = m_detailVector;
                    modifiedId = true;
                    hr = S_OK;
                    break;
                }

                case s_typeId:
                {
                    VariantCopy(result, m_typeString.addressof());
                    modifiedId = true;
                    hr = S_OK;
                    break;
                }
                }
            }
        }
        if (FAILED(hr) && !modifiedId && m_unmodifiedEventArgsAsIDispatch != nullptr)
        {
            DISPID effectiveDispId = dispId;
            if (effectiveDispId > s_dispIdOffset)
            {
                effectiveDispId -= s_dispIdOffset;
            }
            hr = m_unmodifiedEventArgsAsIDispatch->Invoke(
                effectiveDispId, riid, localeId, flags, dispParams, result, excepInfo, argErr);
        }
        return hr;
    }

    // ICoreWebView2PrivateDispatchContainer methods
    // If someone tries to unwrap this IDispatch object there is no directly corresponding
    // winrt object to this aggregate object. Instead since the aggregate object is
    // mostly the event args plus extra properties, we unwrap to the event args.
    HRESULT STDMETHODCALLTYPE GetInnerObject(IUnknown** object) override
    {
        HRESULT hr = DISP_E_MEMBERNOTFOUND;
        if (m_unmodifiedEventArgsAsIDispatch != nullptr)
        {
            hr = S_OK;
            *object = m_unmodifiedEventArgsAsIDispatch.as<IUnknown>().detach();
        }

        return hr;
    }

    HRESULT STDMETHODCALLTYPE GetBasisVariant(VARIANT* basis) override
    {
        VariantCopy(basis, m_unmodifiedEventArgsAsVARIANT.addressof());
        return S_OK;
    }

private:
    winrt::com_ptr<IDispatch> m_source; // The source of the event
    // The event args of the event as an IDispatch.
    winrt::com_ptr<IDispatch> m_unmodifiedEventArgsAsIDispatch;
    UniqueVariant m_unmodifiedEventArgsAsVARIANT;
    UniqueVariant m_detailVector; // The detail property array
    UniqueVariant m_typeString;   // The name of the event

    // The 0 index of the detail array that is the unmodified event args
    UniqueVariant m_detailValue;

    // The DISPID of the known properties
    static const DISPID s_targetId = 1;
    static const DISPID s_detailId = 2;
    static const DISPID s_typeId = 3;
    // The offset for all the DISPIDs on the event args.
    static const DISPID s_dispIdOffset = s_typeId;
};

HRESULT CreateAggregateEventArgs(
    LPCWSTR eventName, IDispatch* source, VARIANT* unmodifiedDelegateParamsAsVARIANT,
    const ULONG unmodifiedDelegateParamsAsVARIANTCount, IDispatch** aggregateEventArgs)
{
    winrt::make<AggregateEventArgs>(
        eventName, source, unmodifiedDelegateParamsAsVARIANT,
        unmodifiedDelegateParamsAsVARIANTCount)
        .as<IDispatch>()
        .copy_to(aggregateEventArgs);

    return S_OK;
}

} // namespace wv2winrt_impl

