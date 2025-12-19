#include "pch.h"

#include "wv2winrt/DispatchAdapter.h"

#include "wv2winrt/dispatchcontainer.h"
#include "wv2winrt/main.h"
#include "DispatchAdapter.g.cpp"

using namespace wv2winrt_impl;

namespace winrt::WinRTAdapter::implementation
{
DispatchAdapter::DispatchAdapter()
{
}

void DispatchAdapter::Clean()
{
    for (auto entry = m_wrappedNamedObjects.begin(); entry != m_wrappedNamedObjects.end(); ++entry)
    {
        if (!entry->second.get())
        {
            m_wrappedNamedObjects.erase(entry);
        }
    }
    for (auto entry = m_wrappedObjects.begin(); entry != m_wrappedObjects.end(); ++entry)
    {
        if (!entry->second.get())
        {
            m_wrappedObjects.erase(entry);
        }
    }
}

winrt::Windows::Foundation::IInspectable DispatchAdapter::WrapObject(
    winrt::Windows::Foundation::IInspectable unwrapped,
    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter dispatchAdapter)
{
    winrt::com_ptr<::IInspectable> unwrappedAsABI = unwrapped.as<::IInspectable>();
    IInspectable wrappedDispatch;
    auto cacheEntry = m_wrappedObjects.find(unwrappedAsABI.get());
    if (cacheEntry != m_wrappedObjects.end())
    {
        // Found entry, attempt to resolve weak pointer.
        wrappedDispatch = cacheEntry->second.get();
    }

    if (!wrappedDispatch)
    {
        // Missing cache entry or couldn't resolve weak pointer. Make new wrapper.
        winrt::com_ptr<::IDispatch> dispatch;
        bool is_cacheable = false;
        HRESULT hr = CreateDispatchFromInspectable(
            unwrappedAsABI.get(), dispatchAdapter, dispatch.put(), &is_cacheable);
        winrt::check_hresult(hr);

        wrappedDispatch = dispatch.as<IInspectable>();
        // Update cache with new wrapper.
        if (is_cacheable)
        {
            m_wrappedObjects[unwrappedAsABI.get()] = winrt::make_weak(wrappedDispatch);
        }
    }

    return wrappedDispatch;
}

winrt::Windows::Foundation::IInspectable DispatchAdapter::WrapNamedObject(
    winrt::hstring name,
    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter dispatchAdapter)
{
    IInspectable wrappedDispatch;
    auto cacheEntry = m_wrappedNamedObjects.find(name);
    if (cacheEntry != m_wrappedNamedObjects.end())
    {
        // Found entry, attempt to resolve weak pointer.
        wrappedDispatch = cacheEntry->second.get();
    }

    if (!wrappedDispatch)
    {
        // Missing cache entry or couldn't resolve weak pointer. Make new wrapper.
        winrt::com_ptr<::IDispatch> dispatch;
        HRESULT hr = CreateDispatchFromName(name.c_str(), dispatchAdapter, dispatch.put());
        winrt::check_hresult(hr);

        wrappedDispatch = dispatch.as<IInspectable>();
        // Update cache with new wrapper.
        m_wrappedNamedObjects[name] = winrt::make_weak(wrappedDispatch);
    }

    return wrappedDispatch;
}

winrt::Windows::Foundation::IInspectable DispatchAdapter::UnwrapObject(
    winrt::Windows::Foundation::IInspectable wrapped)
{
    winrt::com_ptr<IUnknown> unwrappedAsIUnknown;
    winrt::com_ptr<::wv2winrt_impl::ICoreWebView2PrivateDispatchContainer> dispatchContainer;
    dispatchContainer = wrapped.as<::wv2winrt_impl::ICoreWebView2PrivateDispatchContainer>();
    if (dispatchContainer)
    {
        if (SUCCEEDED(dispatchContainer->
            GetInnerObject(unwrappedAsIUnknown.put())))
        {
            return unwrappedAsIUnknown.as<IInspectable>();
        }
    }
    return nullptr;
}

}

