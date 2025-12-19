#pragma once
#include "DispatchAdapter.g.h"

namespace winrt::WinRTAdapter::implementation
{
    struct DispatchAdapter : DispatchAdapterT<DispatchAdapter>
    {
        DispatchAdapter();
        void Clean();

        winrt::Windows::Foundation::IInspectable WrapObject(
            winrt::Windows::Foundation::IInspectable unwrapped,
            winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter dispatchAdapter);

        winrt::Windows::Foundation::IInspectable WrapNamedObject(
            winrt::hstring name,
            winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter dispatchAdapter);

        winrt::Windows::Foundation::IInspectable UnwrapObject(
            winrt::Windows::Foundation::IInspectable wrapped);

    private:
        // Cache of wrapped objects used by WrapObject. This maps unwrapped objects (as void*)
        // to wrapped IDispatch objects (as weak_ref).
        std::map<void*, winrt::weak_ref<winrt::Windows::Foundation::IInspectable> > m_wrappedObjects;
        // Cache of wrapped objects used by WrapNamedObject. This maps names (as hstring)
        // to wrapped IDispatch objects (as weak_ref).
        std::map<winrt::hstring, winrt::weak_ref<winrt::Windows::Foundation::IInspectable> > m_wrappedNamedObjects;
    };
}

namespace winrt::WinRTAdapter::factory_implementation
{
    struct DispatchAdapter : DispatchAdapterT<DispatchAdapter, implementation::DispatchAdapter>
    {
    };
}

