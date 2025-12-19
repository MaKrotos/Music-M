// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#include "pch.h"
// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#include "wv2winrt/asyncdispatch.h"
#include "wv2winrt/uniquevariant.h"

#include <winrt/Windows.Foundation.h>

namespace wv2winrt_impl
{

struct DispatchAsyncResult : winrt::implements<
                                 DispatchAsyncResult, ICoreWebView2PrivateDispatchAsyncResult,
                                 ICoreWebView2PrivateDispatchAsyncResult2,
                                 ICoreWebView2PrivateDispatchAsyncFinishedHandler,
                                 ICoreWebView2PrivateDispatchAsyncInfo>
{
    DispatchAsyncResult()
    {
    }

    // ICoreWebView2PrivateDispatchAsyncResult
    virtual HRESULT STDMETHODCALLTYPE
    SetCompletedHandler(ICoreWebView2PrivateDispatchAsyncFinishedHandler* completedCallback)
    {
        m_completedCallback.copy_from(completedCallback);
        return CheckCompletedCallback();
    }

    // ICoreWebView2PrivateDispatchAsyncFinishedHandler
    HRESULT STDMETHODCALLTYPE Invoke(HRESULT errorCode, VARIANT* result) override
    {
        if (!m_completed)
        {
            m_completed = true;
            if (result != nullptr)
            {
                VariantCopy(m_result.reset_and_addressof(), result);
            }
            else
            {
                m_result.reset();
                m_result.vt = VT_EMPTY;
            }
            m_errorCode = errorCode;
        }
        return CheckCompletedCallback();
    }

    HRESULT STDMETHODCALLTYPE Cancel() override
    {
        if (m_asyncInfo)
        {
            m_asyncInfo.Cancel();
        }
        return S_OK;
    }

    HRESULT STDMETHODCALLTYPE SetAsyncInfo(winrt::Windows::Foundation::IAsyncInfo asyncInfo)
    {
        m_asyncInfo = asyncInfo;
        return S_OK;
    }

private:
    HRESULT CheckCompletedCallback()
    {
        HRESULT hr = S_OK;

        if (m_completed && m_completedCallback && !m_invokeStarted)
        {
            m_invokeStarted = true;
            hr = m_completedCallback->Invoke(m_errorCode, m_result.addressof());
        }
        return hr;
    }

    winrt::com_ptr<ICoreWebView2PrivateDispatchAsyncFinishedHandler> m_completedCallback;
    winrt::Windows::Foundation::IAsyncInfo m_asyncInfo;

    bool m_completed = false;
    bool m_invokeStarted = false;
    UniqueVariant m_result;
    HRESULT m_errorCode = S_OK;
};

HRESULT CreateDispatchAsyncResult(ICoreWebView2PrivateDispatchAsyncResult** asyncResult)
{
    winrt::make<DispatchAsyncResult>().copy_to(asyncResult);
    return S_OK;
}

} // namespace wv2winrt_impl

