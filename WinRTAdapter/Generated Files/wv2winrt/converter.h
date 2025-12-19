// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#pragma once

#include "wv2winrt/remotedictionary.h"
#include "wv2winrt/timehelper.h"

#include <ios>
#include <sstream>

// Must be included before any cppwinrt headers
#include <wv2winrt/base.h>

#include <winrt/Microsoft.Web.WebView2.Core.h>
#include <winrt/Windows.Foundation.Collections.h>
#include <winrt/Windows.Foundation.h>

namespace wv2winrt_impl
{
#define DEFINE_IS_TEMPLATIZED_TYPE_CHECK(WinRTType, Name)                                      \
    template <typename T> struct TIs##Name : public std::false_type                            \
    {                                                                                          \
    };                                                                                         \
    template <typename T> struct TIs##Name<WinRTType<T>> : public std::true_type               \
    {                                                                                          \
    };                                                                                         \
    template <typename T> constexpr bool TIs##Name##_v = TIs##Name<T>::value;

DEFINE_IS_TEMPLATIZED_TYPE_CHECK(
    winrt::Windows::Foundation::Collections::IVectorView, VectorView);
DEFINE_IS_TEMPLATIZED_TYPE_CHECK(winrt::Windows::Foundation::Collections::IVector, Vector);
DEFINE_IS_TEMPLATIZED_TYPE_CHECK(winrt::array_view, ArrayView);
DEFINE_IS_TEMPLATIZED_TYPE_CHECK(winrt::com_array, ComArray);
#undef DEFINE_IS_TEMPLATIZED_TYPE_CHECK

template <typename T> T GetDefaultValue()
{
    if constexpr (std::is_integral_v<T> || std::is_floating_point_v<T>)
    {
        return 0;
    }
    else if constexpr (std::is_same_v<T, bool>)
    {
        return false;
    }
    else if constexpr (std::is_constructible_v<T>)
    {
        return {};
    }
    else
    {
        return {nullptr};
    }
}

// Some helpers to make it easier to read and write the below Converter specializations.
// normalize_t<T> will turn const T& into T, is_similar_v<T1, T2> is true when T1 and T2
// are the same after normalize_t. We have many Converter specializations that do the same
// thing for and need to be defined for both T and const T&.
template <typename T> using normalize_t = std::remove_const_t<std::remove_reference_t<T>>;
template <typename T1, typename T2>
using is_similar = std::is_same<normalize_t<T1>, normalize_t<T2>>;
template <typename T1, typename T2> constexpr bool is_similar_v = is_similar<T1, T2>::value;
template <typename T1, typename T2>
using variant_if_similar_t = std::enable_if_t<is_similar_v<T1, T2>, VARIANT>;

// MemberContainer just contains a member of a certain type. But not all types have
// default constructors. So we use the is_constructible to figure out if it does and
// if so pick the template specialization that uses default construction or otherwise
// uses nullptr_t initialization. Most smart types allow for default construction
// but the winrt types do not and require explicit {nullptr} init.
template <typename TMember, bool HasDefaultCtor = std::is_constructible<TMember>::value>
struct MemberContainer;

template <typename TMember> struct MemberContainer<TMember, true>
{
    TMember value;
};

template <typename TMember> struct MemberContainer<TMember, false>
{
    TMember value{nullptr};
};

// MemberContainerArray is similar to MemberContainer but for arrays. Unlike
// MemberContainerOutArray, it does not require a size to be passed in the constructor
// and initializes an empty array.
template <typename TMember, bool HasDefaultCtor = std::is_constructible<TMember>::value>
struct MemberContainerArray;

template <typename TMember> struct MemberContainerArray<TMember, true>
{
    winrt::com_array<TMember> value;
};

template <typename TMember> struct MemberContainerArray<TMember, false>
{
    winrt::com_array<TMember> value{nullptr};
};

// MemberContainerOutArray is similar to MemberContainerArray but requires a size to be
// passed in the constructor and initializes an com_array of that size.
template <typename TMember, bool HasDefaultCtor = std::is_constructible<TMember>::value>
struct MemberContainerOutArray;

template <typename TMember> struct MemberContainerOutArray<TMember, true>
{
    MemberContainerOutArray(uint32_t size) : value(size)
    {
    }
    winrt::com_array<TMember> value;
};

template <typename TMember> struct MemberContainerOutArray<TMember, false>
{
    MemberContainerOutArray(uint32_t size) : value{size, nullptr}
    {
    }
    winrt::com_array<TMember> value;
};

template <typename TIn, typename TOut> struct Converter;

// RefConverter just does the normal Converter functionality but obtaining the in value is
// separate from and requires storage before later converting to the out value. Call
// ConvertRef with where to store the result of the conversion and ConvertRef will return a
// pointer to where the input should be stored. When the object destructs it will perform
// the conversion. This is intended to be used in a method call as a temporary object like:
//      VARIANT indexAsVariant;
//      Uri.GetIndex("find", RefConverter<uint32_t,
//      VARIANT>(dispatchAdapter).ConvertRef(&indexAsVariant));
// In the above GetIndex has a uint32_t out parameter for its second parameter. We receive
// the value from the out param via the return value of ConvertRef. When the RefConverter
// destructs (which occurs after the statement completes) the conversion happens and is
// stored into indexAsVariant.
//
template <typename TIn, typename TOut> struct RefConverter
{
    RefConverter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
                     dispatchAdapter)
        : m_dispatchAdapter(dispatchAdapter)
    {
    }
    ~RefConverter()
    {
        *m_out = Converter<TIn, TOut>(m_dispatchAdapter).Convert(m_in.value);
    }

    TIn& ConvertRef(TOut* out)
    {
        m_out = out;
        m_in.value = Converter<TOut, TIn>(m_dispatchAdapter).Convert(*out);
        return m_in.value;
    }

    TIn& ConvertOut(TOut* out)
    {
        m_out = out;
        return m_in.value;
    }

    TIn& ConvertOutRef(TOut* out)
    {
        m_out = out;
        return m_in.value;
    }

private:
    MemberContainer<TIn> m_in;
    TOut* m_out;

    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
};

template <typename TInChild> struct RefConverter<winrt::array_view<TInChild>, VARIANT>
{
    using TIn = winrt::com_array<TInChild>;
    using TInArrayView = winrt::array_view<TInChild>;
    using TOut = VARIANT;

    RefConverter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
                     dispatchAdapter)
        : m_dispatchAdapter(dispatchAdapter)
    {
    }
    ~RefConverter()
    {
        if (!m_isInOutArray)
        {
            *m_out = Converter<TIn, TOut>(m_dispatchAdapter).Convert(m_in.value);
        }
        else
        {
            *m_out = Converter<TIn, TOut>(m_dispatchAdapter).Convert(m_arrayPointer->value);
            delete m_arrayPointer;
        }
    }

    TIn& ConvertRef(TOut* out)
    {
        m_out = out;
        return m_in.value;
    }

    TInArrayView ConvertOut(TOut* out)
    {
        m_out = out;
        return m_in.value;
    }

    TInArrayView ConvertInOut(VARIANT arr, TOut* out)
    {
        // Used for In Out Array parameters in order to get the size of the passed
        // in array and allocate the new out winrt::com_array.
        m_out = out;
        if (arr.vt & VT_ARRAY)
        {
            SAFEARRAY* pArray = arr.parray;
            long lBound, uBound;
            SafeArrayGetLBound(pArray, 1, &lBound);
            SafeArrayGetUBound(pArray, 1, &uBound);
            long d = uBound - lBound + 1;
            uint32_t size = static_cast<uint32_t>(d);
            m_arrayPointer = new MemberContainerOutArray<TInChild>(size);
            m_isInOutArray = true;
            return m_arrayPointer->value;
        }

        return m_in.value;
    }

    TIn& ConvertOutRef(TOut* out)
    {
        m_out = out;
        return m_in.value;
    }

private:
    // m_in is used to store the initialized com_array if no array is passed in.
    // m_arrayPointer is used to store the initialized com_array if an array is
    // passed in, it handles in out arrays.
    // m_out is used to store the result array after the method completes.
    MemberContainerArray<TInChild> m_in;
    MemberContainerOutArray<TInChild>* m_arrayPointer = nullptr;
    TOut* m_out;
    bool m_isInOutArray = false;

    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
};

// The Converter template converts values of type TIn to corresponding value of TOut.
// We use template specialization to customize the conversion behavior for particular types.
// The default non-specialized case just tries to cast.
template <typename TIn, typename TOut> struct Converter
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
                  dispatchAdapter)
    {
    }
    TOut Convert(const TIn& value)
    {
        return static_cast<const TOut>(value);
    }
};

// A bunch of VARIANT types can be implemented the same via this macro
#define CONVERTER_VARIANT_TO_FROM_TYPE(winrtType, vtType, variantProperty)                     \
    static_assert(sizeof(winrtType) == sizeof(decltype(VARIANT().variantProperty)));           \
    static_assert(                                                                             \
        std::is_signed_v<winrtType> == std::is_signed_v<decltype(VARIANT().variantProperty)>); \
                                                                                               \
    template <typename TIn> struct Converter<TIn, variant_if_similar_t<TIn, winrtType>>        \
    {                                                                                          \
        Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&)  \
        {                                                                                      \
        }                                                                                      \
        VARIANT Convert(winrtType value)                                                       \
        {                                                                                      \
            VARIANT result{vtType};                                                            \
            result.variantProperty = value;                                                    \
            return result;                                                                     \
        }                                                                                      \
    };

CONVERTER_VARIANT_TO_FROM_TYPE(uint8_t, VT_UI1, bVal)
CONVERTER_VARIANT_TO_FROM_TYPE(int16_t, VT_I2, iVal)
CONVERTER_VARIANT_TO_FROM_TYPE(uint16_t, VT_UI2, uiVal)
CONVERTER_VARIANT_TO_FROM_TYPE(int32_t, VT_I4, lVal)
CONVERTER_VARIANT_TO_FROM_TYPE(uint32_t, VT_UI4, ulVal)
CONVERTER_VARIANT_TO_FROM_TYPE(int64_t, VT_I8, llVal)
CONVERTER_VARIANT_TO_FROM_TYPE(uint64_t, VT_UI8, ullVal)
CONVERTER_VARIANT_TO_FROM_TYPE(float, VT_R4, fltVal)
CONVERTER_VARIANT_TO_FROM_TYPE(double, VT_R8, dblVal)

#undef CONVERTER_VARIANT_TO_FROM_TYPE

// Convert variant to integers
template <typename TInt>
struct Converter<
    std::enable_if_t<
        std::is_integral_v<std::remove_reference_t<TInt>> ||
            std::is_floating_point_v<std::remove_reference_t<TInt>>,
        VARIANT>,
    TInt>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
                  dispatchAdapter)
        : m_dispatchAdapter(dispatchAdapter)
    {
    }

    TInt Convert(VARIANT value)
    {
        if (value.vt & VT_ARRAY)
        {
            winrt::com_array<TInt> array =
                Converter<VARIANT, winrt::com_array<TInt>>(m_dispatchAdapter).Convert(value);
            return (array.size() == 1) ? array[0] : 0;
        }
#define CONVERTER_INTEGRAL_CASE(vtType, variantProperty)                                       \
    case vtType:                                                                               \
        /* For integral types, we need to cast to uint64_t first    */                         \
        /* to ensure we get the correct conversion for large values */                         \
        if (std::is_integral_v<TInt>)                                                          \
            return static_cast<TInt>(static_cast<uint64_t>(value.variantProperty));            \
        return static_cast<TInt>(value.variantProperty);

        switch (value.vt)
        {
            CONVERTER_INTEGRAL_CASE(VT_I1, cVal)
            CONVERTER_INTEGRAL_CASE(VT_UI1, bVal)
            CONVERTER_INTEGRAL_CASE(VT_I2, iVal)
            CONVERTER_INTEGRAL_CASE(VT_UI2, uiVal)
            CONVERTER_INTEGRAL_CASE(VT_I4, lVal)
            CONVERTER_INTEGRAL_CASE(VT_UI4, ulVal)
            CONVERTER_INTEGRAL_CASE(VT_I8, llVal)
            CONVERTER_INTEGRAL_CASE(VT_UI8, ullVal)
            CONVERTER_INTEGRAL_CASE(VT_R4, fltVal)
            CONVERTER_INTEGRAL_CASE(VT_R8, dblVal)
#undef CONVERTER_INTEGRAL_CASE
        case VT_BOOL:
            return static_cast<TInt>((value.boolVal == VARIANT_TRUE) ? 1 : 0);
        case VT_DATE:
            // This will be in JS Date format - milliseconds from UNIX epoch
            return static_cast<TInt>(VariantTimeToJsTime(value.date));
        case VT_DISPATCH:
        case VT_BSTR:
        {
            try
            {
                std::wstring stringValue =
                    Converter<VARIANT, std::wstring>(m_dispatchAdapter).Convert(value);
                TInt result = 0;
                size_t index = 0;
                if (stringValue.find('.') != std::string::npos)
                {
                    // If the string contains a decimal, try converting the string to a double.
                    result = static_cast<decltype(result)>(std::stod(stringValue, &index));
                }
                else
                {
                    // Otherwise, try converting the string to an integer.
                    result = static_cast<decltype(result)>(std::stoi(stringValue, &index));
                    if (result == 0 && index == 1)
                    {
                        // If stoi got zero, check to see if stringValue had a prefix for bin,
                        // oct, or hex.
                        std::wstring prefix = stringValue.substr(0, 2);
                        if (prefix.compare(L"0x") == 0 || prefix.compare(L"0X") == 0)
                        {
                            result = static_cast<decltype(result)>(
                                std::stoi(stringValue.substr(2), &index, 16));
                        }
                        else if (prefix.compare(L"0b") == 0 || prefix.compare(L"0B") == 0)
                        {
                            result = static_cast<decltype(result)>(
                                std::stoi(stringValue.substr(2), &index, 2));
                        }
                        else if (prefix.compare(L"0o") == 0 || prefix.compare(L"0O") == 0)
                        {
                            result = static_cast<decltype(result)>(
                                std::stoi(stringValue.substr(2), &index, 8));
                        }
                        // Increment index for offset of the substring above. If none of the
                        // above conditions were met, then result is already 0.
                        index += 2;
                    }
                }
                // If index is not the length of the string, then there are characters in the
                // string that couldn't be converted to an int. This makes the string an invalid
                // int and should return 0.
                return (index == stringValue.length()) ? result : 0;
            }
            catch (...)
            {
                return 0;
            }
        }
        default:
            return 0;
        }
    }
    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
};

// Converting any cpp/winrt IUnknown/IInspectable object to an IDispatch VARIANT
// enable_if_t takes a bool as the first template param and resolves to the second template
// param type. In our case it resolves to a VARIANT and the bool is if our TIn type is
// derived from IUnknown. The conditional bool isn't really related to the VARIANT type and
// it more belongs on the TIn, but C++ doesn't want us to use TIn as the type that
// enable_if_t resolves to. The is_base_of_v template will check if the second template
// param is derived from the first. We use it here to determine if TIn is derived from the
// c++/winrt IUnknown type or if TIn is a reference to a c++/winrt IUnknown derived type. We
// do this using the remove_reference_t template which given a T& type will give out T type.
template <typename TIn>
struct Converter<
    TIn,
    std::enable_if_t<
        (std::is_base_of_v<winrt::Windows::Foundation::IUnknown, TIn> ||
         std::is_base_of_v<winrt::Windows::Foundation::IUnknown, std::remove_reference_t<TIn>>),
        VARIANT>>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
                  dispatchAdapter)
        : m_dispatchAdapter(dispatchAdapter)
    {
    }
    VARIANT Convert(const TIn& value)
    {
        if (!value)
        {
            VARIANT resultAsVariant{VT_EMPTY};
            return resultAsVariant;
        }
        if (auto valueAsIPropertyValue =
                value.try_as<winrt::Windows::Foundation::IPropertyValue>())
        {
            auto resultAsValueVariant =
                Converter<decltype(valueAsIPropertyValue), VARIANT>(m_dispatchAdapter)
                    .Convert(valueAsIPropertyValue);
            // Boxed WinRT structs implement IPropertyValue but can't get arbitrary types out of
            // it, the Converter will return VT_NULL for them, and we'll let dispatch adapter
            // unbox the struct.
            if (resultAsValueVariant.vt != VT_NULL)
            {
                return resultAsValueVariant;
            }
        }

        winrt::Windows::Foundation::IInspectable valueAsInspectable;
        winrt::Windows::Foundation::IInspectable resultAsInspectable;
        winrt::com_ptr<IDispatch> resultAsDispatch;
        VARIANT resultAsVariant{VT_DISPATCH};

        valueAsInspectable = value.as<decltype(valueAsInspectable)>();
        resultAsInspectable =
            m_dispatchAdapter.WrapObject(valueAsInspectable, m_dispatchAdapter);
        resultAsDispatch = resultAsInspectable.as<::IDispatch>();

        resultAsVariant.pdispVal = resultAsDispatch.detach();
        return resultAsVariant;
    }

    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
};

template <typename> struct is_ireference : std::false_type
{
};

template <typename TRef>
struct is_ireference<winrt::Windows::Foundation::IReference<TRef>> : std::true_type
{
};

template <typename TOut>
struct Converter<
    std::enable_if_t<
        (std::is_base_of_v<winrt::Windows::Foundation::IUnknown, TOut> ||
         std::is_base_of_v<
             winrt::Windows::Foundation::IUnknown,
             std::remove_reference_t<TOut>>)&&!TIsVectorView_v<TOut> &&
            !TIsVector_v<TOut> && !is_ireference<TOut>::value,
        VARIANT>,
    TOut>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
                  dispatchAdapter)
        : m_dispatchAdapter(dispatchAdapter)
    {
    }
    TOut Convert(const VARIANT& valueAsVariant)
    {
        if (valueAsVariant.vt == VT_NULL || valueAsVariant.vt == VT_EMPTY)
        {
            return {nullptr};
        }
        else if (valueAsVariant.vt == VT_DISPATCH || valueAsVariant.vt == VT_UNKNOWN)
        {
            winrt::com_ptr<IInspectable> valueAsComInspectable;
            winrt::Windows::Foundation::IInspectable resultAsInspectable;
            winrt::check_hresult(valueAsVariant.pdispVal->QueryInterface(
                IID_PPV_ARGS(valueAsComInspectable.put())));
            winrt::Windows::Foundation::IInspectable valueAsInspectable = {
                valueAsComInspectable.as<winrt::Windows::Foundation::IInspectable>()};

            resultAsInspectable = m_dispatchAdapter.UnwrapObject(valueAsInspectable);
            return resultAsInspectable.as<TOut>();
        }
        else
        {
            if constexpr (
                std::is_same_v<TOut, winrt::Windows::Foundation::IPropertyValue> ||
                std::is_same_v<TOut, winrt::Windows::Foundation::IInspectable>)
            {
                return Converter<VARIANT, winrt::Windows::Foundation::IPropertyValue>(
                           m_dispatchAdapter)
                    .Convert(valueAsVariant);
            }
            else
            {
                winrt::throw_hresult(DISP_E_BADVARTYPE);
            }
        }
    }

    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
};

// Convert from VARIANT to cpp/winrt type
template <typename TIn> struct Converter<TIn, variant_if_similar_t<TIn, std::wstring>>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&)
    {
    }
    VARIANT Convert(const std::wstring in)
    {
        VARIANT out{VT_BSTR};
        out.bstrVal = SysAllocString(in.c_str());
        return out;
    };
};

// Convert to/from arrays
// Vector/VectorView -> VARIANT is covered by normal IUnknown -> VARIANT dispatch wrapper above.
template <typename TInChild> struct Converter<winrt::com_array<TInChild>, VARIANT>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
                  dispatchAdapter)
        : m_dispatchAdapter(dispatchAdapter)
    {
    }
    VARIANT Convert(const winrt::com_array<TInChild>& value)
    {
        return Converter<winrt::array_view<TInChild>, VARIANT>(m_dispatchAdapter)
            .Convert(value);
    }
    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
};

template <typename TInChild> struct Converter<winrt::array_view<TInChild>, VARIANT>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
                  dispatchAdapter)
        : m_dispatchAdapter(dispatchAdapter)
    {
    }
    VARIANT Convert(const winrt::array_view<TInChild>& value)
    {
        const uint32_t size = value.size();
        SAFEARRAY* array = SafeArrayCreateVector(VT_VARIANT, 0, size);
        winrt::check_pointer(array);

        for (uint32_t index = 0; index < size; ++index)
        {
            VARIANT entryAsVariant =
                Converter<TInChild, VARIANT>(m_dispatchAdapter).Convert(value.at(index));
            LONG indexAsLong = index;
            HRESULT hr = SafeArrayPutElement(array, &indexAsLong, &entryAsVariant);
            winrt::check_hresult(hr);
        }

        VARIANT arrayAsVariant;
        arrayAsVariant.vt = VT_ARRAY | VT_VARIANT;
        arrayAsVariant.parray = array;
        return arrayAsVariant;
    }
    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
};

template <typename TOutChild> struct Converter<VARIANT, winrt::array_view<TOutChild>>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
                  dispatchAdapter)
        : m_dispatchAdapter(dispatchAdapter)
    {
    }

    winrt::array_view<TOutChild> Convert(VARIANT value)
    {
        m_result =
            Converter<VARIANT, winrt::com_array<TOutChild>>(m_dispatchAdapter).Convert(value);

        return m_result;
    }

    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
    // We must store the com_array here. A com_array owns its memory (CoTaskMemAlloc) and
    // deletes it when the com_array goes out of scope. The array_view doesn't own memory
    // and is more like a view into something else. Accordingly we must keep the com_array
    // here so that the array_view has something to reference. Otherwise it goes out of
    // scope and the contents are deleted. This is further confusing because com_array is
    // derived from array_view but they don't have a virtual destructor.
    winrt::com_array<TOutChild> m_result;
};

template <typename TOutChild> struct Converter<VARIANT, winrt::com_array<TOutChild>>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
                  dispatchAdapter)
        : m_dispatchAdapter(dispatchAdapter)
    {
    }
    winrt::com_array<TOutChild> Convert(VARIANT value)
    {
        auto vector =
            Converter<VARIANT, winrt::Windows::Foundation::Collections::IVector<TOutChild>>(
                m_dispatchAdapter)
                .Convert(value);
        return {vector.begin(), vector.end()};
    }

    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
};

struct BoolWidener
{
    BoolWidener(bool value) : m_data(value)
    {
    }
    operator bool() const
    {
        return m_data;
    }
    operator bool*()
    {
        return &m_data;
    }
    operator const bool*() const
    {
        return &m_data;
    }

    BoolWidener& operator=(const bool value)
    {
        m_data = value;
        return *this;
    }
    union
    {
        bool m_data;
        BYTE m_singleByte;
    };
};

struct BoolVector : winrt::implements<
                        BoolVector, winrt::Windows::Foundation::Collections::IVector<bool>,
                        winrt::Windows::Foundation::Collections::IVectorView<bool>,
                        winrt::Windows::Foundation::Collections::IIterable<bool>>,
                    winrt::vector_base<BoolVector, bool>
{
    auto& get_container() const noexcept
    {
        return m_values;
    }
    auto& get_container() noexcept
    {
        return m_values;
    }

private:
    std::vector<BoolWidener> m_values;
};

template <typename TOut, typename TOutChild> struct ConverterToVectorHelper
{
    ConverterToVectorHelper(
        const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
            dispatchAdapter)
        : m_dispatchAdapter(dispatchAdapter)
    {
    }
    TOut Convert(VARIANT value)
    {
        if ((value.vt & VT_ARRAY) != 0)
        {
            SAFEARRAY* safeArray =
                (value.vt == (VT_BYREF | VT_ARRAY)) ? *value.pparray : value.parray;
            DWORD dimension = SafeArrayGetDim(safeArray);
            if (dimension != 1)
            {
                winrt::throw_hresult(E_INVALIDARG);
            }

            LONG lowerBound = 0; // lower bound of array
            LONG upperBound = 0; // upper bound of array
            SafeArrayGetLBound(safeArray, dimension, &lowerBound);
            SafeArrayGetUBound(safeArray, dimension, &upperBound);
            VARTYPE childVarType;
            HRESULT hr = SafeArrayGetVartype(safeArray, &childVarType);
            winrt::check_hresult(hr);
            if (childVarType != VT_VARIANT)
            {
                winrt::throw_hresult(E_INVALIDARG);
            }

            const UINT elementSize = SafeArrayGetElemsize(safeArray);
            if (elementSize != sizeof(VARIANT))
            {
                winrt::throw_hresult(E_ABORT);
            }

            winrt::Windows::Foundation::Collections::IVector<TOutChild> collection;
            if constexpr (!std::is_same_v<TOutChild, bool>)
            {
                if constexpr (std::is_same_v<
                                  winrt::Windows::Foundation::Collections::IObservableVector<
                                      TOutChild>,
                                  TOut>)
                {
                    collection = winrt::single_threaded_observable_vector<TOutChild>();
                }
                else
                {
                    collection = winrt::single_threaded_vector<TOutChild>();
                }
            }
            else
            {
                collection = winrt::make<BoolVector>();
            }

            for (LONG index = lowerBound; index <= upperBound; ++index)
            {
                VARIANT entryAsVariant;
                hr = SafeArrayGetElement(
                    safeArray, &index, reinterpret_cast<void*>(&entryAsVariant));
                winrt::check_hresult(hr);

                auto entryAsOutType =
                    Converter<VARIANT, TOutChild>(m_dispatchAdapter).Convert(entryAsVariant);

                collection.Append(entryAsOutType);

                VariantClear(&entryAsVariant);
            }
            return collection.as<TOut>();
        }
        else if (value.vt == VT_DISPATCH || value.vt == VT_UNKNOWN)
        {
            // If the VARIANT represents a wrapped IDispatch object, then we need to extract
            // the inner object, but it must implement the specified IVector interface.
            winrt::com_ptr<IInspectable> valueAsComInspectable;
            winrt::Windows::Foundation::IInspectable resultAsInspectable;
            winrt::check_hresult(
                value.pdispVal->QueryInterface(IID_PPV_ARGS(valueAsComInspectable.put())));
            winrt::Windows::Foundation::IInspectable valueAsInspectable = {
                valueAsComInspectable.as<winrt::Windows::Foundation::IInspectable>()};

            resultAsInspectable = m_dispatchAdapter.UnwrapObject(valueAsInspectable);
            return resultAsInspectable.as<TOut>();
        }
        winrt::throw_hresult(E_INVALIDARG);
    }

    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
};

#define VARIANT_TO_VECTOR_TYPE_HELPER(VectorType)                                              \
    template <typename TOutChild>                                                              \
    struct Converter<VARIANT, winrt::Windows::Foundation::Collections::VectorType<TOutChild>>  \
    {                                                                                          \
        using TOut = winrt::Windows::Foundation::Collections::VectorType<TOutChild>;           \
        Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&   \
                      dispatchAdapter)                                                         \
            : m_converterHelper(dispatchAdapter)                                               \
        {                                                                                      \
        }                                                                                      \
        TOut Convert(VARIANT value)                                                            \
        {                                                                                      \
            return m_converterHelper.Convert(value);                                           \
        }                                                                                      \
        ConverterToVectorHelper<TOut, TOutChild> m_converterHelper;                            \
    };

VARIANT_TO_VECTOR_TYPE_HELPER(IVector);
VARIANT_TO_VECTOR_TYPE_HELPER(IVectorView);
VARIANT_TO_VECTOR_TYPE_HELPER(IObservableVector);
#undef VARIANT_TO_VECTOR_TYPE_HELPER

// Convert from VARIANT to cppwinrt type
template <> struct Converter<VARIANT, std::wstring>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
                  dispatchAdapter)
        : m_dispatchAdapter(dispatchAdapter)
    {
    }
    std::wstring Convert(const VARIANT& value)
    {
        if (value.vt & VT_ARRAY)
        {
            winrt::com_array<winrt::hstring> array =
                Converter<VARIANT, winrt::com_array<winrt::hstring>>(m_dispatchAdapter)
                    .Convert(value);
            std::wstring result;
            for (auto it = array.begin(); it != array.end(); ++it)
            {
                if (it != array.begin())
                    result.append(L",");
                result.append(*it);
            }
            return result;
        }
#define CONVERTER_INTEGRAL_CASE(vtType, variantProperty)                                       \
    case vtType:                                                                               \
        return std::to_wstring(value.variantProperty);

        switch (value.vt)
        {
            CONVERTER_INTEGRAL_CASE(VT_I1, cVal)
            CONVERTER_INTEGRAL_CASE(VT_UI1, bVal)
            CONVERTER_INTEGRAL_CASE(VT_I2, iVal)
            CONVERTER_INTEGRAL_CASE(VT_UI2, uiVal)
            CONVERTER_INTEGRAL_CASE(VT_I4, lVal)
            CONVERTER_INTEGRAL_CASE(VT_UI4, ulVal)
            CONVERTER_INTEGRAL_CASE(VT_I8, llVal)
            CONVERTER_INTEGRAL_CASE(VT_UI8, ullVal)
#undef CONVERTER_INTEGRAL_CASE

        case VT_R4:
        case VT_R8:
        {
            // Floats and doubles need to use ostringstream instead of to_string
            // to get formatting correctly. Using to_string for floats and doubles
            // can cause trailing zeroes (i.e. 1.875 becomes 1.875000)
            std::wostringstream oss;
            oss << std::noshowpoint << ((value.vt == VT_R8) ? value.dblVal : value.fltVal);
            return oss.str();
        }
        case VT_LPWSTR:
        case VT_BSTR:
            return value.bstrVal;
        case VT_EMPTY:
        case VT_NULL:
            return L"null";
        case VT_BOOL:
            return (value.boolVal == VARIANT_TRUE) ? L"true" : L"false";
        case VT_DISPATCH:
        {
            wchar_t kToStringProperty[] = L"toString";
            LPOLESTR names = kToStringProperty;
            DISPID dispid = 0;
            HRESULT hr = value.pdispVal->GetIDsOfNames(
                IID_NULL, &names, 1, LOCALE_USER_DEFAULT, &dispid);
            if (SUCCEEDED(hr))
            {
                VARIANTARG vararg{VT_BSTR};
                vararg.bstrVal = kToStringProperty;
                DISPPARAMS dispParams;
                dispParams.rgvarg = &vararg;
                dispParams.cArgs = 1;
                EXCEPINFO excepInfo;
                VARIANT result;
                hr = value.pdispVal->Invoke(
                    dispid, IID_NULL, LOCALE_USER_DEFAULT, DISPATCH_METHOD, &dispParams,
                    &result, &excepInfo, NULL);
                if (SUCCEEDED(hr))
                {
                    return Converter<VARIANT, std::wstring>(m_dispatchAdapter).Convert(result);
                }
                else
                {
                    // The object does not support toString as a method.
                    winrt::throw_hresult(E_INVALIDARG);
                }
            }
            // If we aren't able to query the object for a toString property, then return
            // the object string.
            return L"[object Object]";
        }
        default:
            winrt::throw_hresult(E_INVALIDARG);
        }
    }
    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
};

template <> struct Converter<VARIANT, winrt::hstring>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
                  dispatchAdapter)
        : m_dispatchAdapter(dispatchAdapter)
    {
    }
    winrt::hstring Convert(const VARIANT& value)
    {
        return Converter<VARIANT, std::wstring>(m_dispatchAdapter).Convert(value).c_str();
    }

    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
};

template <typename TIn> struct Converter<TIn, variant_if_similar_t<TIn, winrt::hstring>>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&)
    {
    }
    VARIANT Convert(const winrt::hstring& value)
    {
        VARIANT result{VT_BSTR};
        result.bstrVal = SysAllocString(value.data());
        return result;
    }
};

template <> struct Converter<VARIANT, bool>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&)
    {
    }
    bool Convert(const VARIANT& value)
    {
        if (value.vt & VT_ARRAY)
        {
            return true;
        }
        else
        {
            bool result = false;

#define CONVERTER_INTEGRAL_CASE(vtType, variantProperty)                                       \
    case vtType:                                                                               \
        result = !(value.variantProperty == 0 || value.variantProperty == -0);                 \
        break;

            switch (value.vt)
            {
                CONVERTER_INTEGRAL_CASE(VT_I1, cVal)
                CONVERTER_INTEGRAL_CASE(VT_UI1, bVal)
                CONVERTER_INTEGRAL_CASE(VT_I2, iVal)
                CONVERTER_INTEGRAL_CASE(VT_UI2, uiVal)
                CONVERTER_INTEGRAL_CASE(VT_I4, lVal)
                CONVERTER_INTEGRAL_CASE(VT_UI4, ulVal)
                CONVERTER_INTEGRAL_CASE(VT_I8, llVal)
                CONVERTER_INTEGRAL_CASE(VT_UI8, ullVal)
#undef CONVERTER_INTEGRAL_CASE

            case VT_BOOL:
                // VARIANT_FALSE == 0 and FALSE == 0
                // VARIANT_TRUE == -1 and TRUE == 1
                // Because true values don't match we just compare against false
                result = value.boolVal != VARIANT_FALSE;
                break;
            case VT_R4:
                result = !(std::isnan(value.fltVal) || value.fltVal == 0 || value.fltVal == -0);
                break;

            case VT_R8:
                result = !(std::isnan(value.dblVal) || value.dblVal == 0 || value.dblVal == -0);
                break;

            case VT_BSTR:
            case VT_LPWSTR:
                result = !(value.bstrVal[0] == L'\0' || value.bstrVal == nullptr);
                break;

            case VT_EMPTY:
            case VT_NULL:
                result = false;
                break;

            case VT_DISPATCH:
            case VT_DATE:
                result = true;
                break;

            default:
                winrt::throw_hresult(E_INVALIDARG);
            }
            return result;
        }
    }
};

template <typename TIn> struct Converter<TIn, variant_if_similar_t<bool, TIn>>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&)
    {
    }
    VARIANT Convert(const bool& value)
    {
        VARIANT result{VT_BOOL};
        // Note we must use VARIANT_TRUE and VARIANT_FALSE not TRUE, FALSE since
        // the true values don't match (see above)
        result.boolVal = value ? VARIANT_TRUE : VARIANT_FALSE;
        return result;
    }
};

template <> struct Converter<VARIANT, char16_t>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
                  dispatchAdapter)
        : m_dispatchAdapter(dispatchAdapter)
    {
    }

    char16_t Convert(const VARIANT& value)
    {
        std::wstring result =
            Converter<VARIANT, std::wstring>(m_dispatchAdapter).Convert(value);

        if (result.length() != 1)
        {
            winrt::throw_hresult(E_INVALIDARG);
        }
        return result.at(0);
    }
    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
};

template <typename TIn> struct Converter<TIn, variant_if_similar_t<char16_t, TIn>>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&)
    {
    }

    VARIANT Convert(const char16_t& value)
    {
        VARIANT result{VT_BSTR};
        result.bstrVal = SysAllocString(std::wstring(1, value).c_str());
        return result;
    }
};

// To match chakra we support GUIDs with and without {} delimiters as input
// and produce GUIDs with {} delimiters as output
template <> struct Converter<VARIANT, winrt::guid>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&)
    {
    }
    winrt::guid Convert(const VARIANT& value)
    {
        if (value.vt == VT_BSTR || value.vt == VT_LPWSTR)
        {

            // We need to convert our string to a winrt::guid. winrt::guid has a
            // constructor that takes a string and does this conversion, however
            // if it fails to parse it calls abort which is not acceptable. And
            // it requires the incoming string to not have delimiting {}.
            // So instead we use IIDFromString to parse and then create a
            // winrt::guid from that. IIDFromString requires the IID to have
            // delimiting {}s which we don't. So if the string doesn't start with
            // { we copy the string into a buffer with delimting braces.
            static const size_t guidWithBracesAndWithNullLength =
                ARRAYSIZE(L"{e0f5602a-f641-4889-a09e-71c9e1642dd5}");
            static const size_t guidWithoutBracesAndWithNullLength =
                ARRAYSIZE(L"e0f5602a-f641-4889-a09e-71c9e1642dd5");
            std::wstring guidBuffer;

            const wchar_t* valueStr = value.bstrVal;
            size_t valueStrWithNullLength = wcslen(value.bstrVal) + 1;

            if (valueStr[0] != L'{' &&
                valueStrWithNullLength == guidWithoutBracesAndWithNullLength)
            {
                guidBuffer = L'{';
                guidBuffer.append(valueStr);
                guidBuffer += L'}';
                valueStr = guidBuffer.c_str();
                valueStrWithNullLength = guidBuffer.length();
            }

            IID parsedGuidAsIID;
            if (FAILED(IIDFromString(valueStr, &parsedGuidAsIID)))
            {
                winrt::throw_hresult(E_INVALIDARG);
            }

            return winrt::guid{
                (uint32_t)parsedGuidAsIID.Data1, (uint16_t)parsedGuidAsIID.Data2,
                (uint16_t)parsedGuidAsIID.Data3,
                std::array<uint8_t, 8>{
                    parsedGuidAsIID.Data4[0],
                    parsedGuidAsIID.Data4[1],
                    parsedGuidAsIID.Data4[2],
                    parsedGuidAsIID.Data4[3],
                    parsedGuidAsIID.Data4[4],
                    parsedGuidAsIID.Data4[5],
                    parsedGuidAsIID.Data4[6],
                    parsedGuidAsIID.Data4[7],
                }};
        }
        winrt::throw_hresult(E_INVALIDARG);
    }
};

// Convert from VARIANT to cpp/winrt type
template <> struct Converter<VARIANT, winrt::Windows::Foundation::TimeSpan>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
                  dispatchAdapter)
        : m_dispatchAdapter(dispatchAdapter)
    {
    }
    winrt::Windows::Foundation::TimeSpan Convert(const VARIANT& value)
    {
        switch (value.vt)
        {
        // We need to include the specific allowed VTs below instead
        // of just relying on the number converter because the default
        // case from the converter returns 0, whereas here we want
        // to throw an invalidarg exception for the default case.
        case VT_I1:
        case VT_UI1:
        case VT_I2:
        case VT_UI2:
        case VT_I4:
        case VT_UI4:
        case VT_I8:
        case VT_UI8:
        case VT_R4:
        case VT_R8:
        case VT_DATE:
        {
            // For the general number case, the value being sent from JS is assumed to be
            // in milliseconds, which is the default tick format of Date objects. All we
            // need to do is convert to hundred-nanosecond ticks for TimeSpan.
            double milliseconds = Converter<VARIANT, double>(m_dispatchAdapter).Convert(value);
            int64_t hundredNanoseconds =
                std::llround(milliseconds * kHundredNanosecondsPerMillisecond);
            return winrt::Windows::Foundation::TimeSpan(hundredNanoseconds);
        }
        default:
            winrt::throw_hresult(E_INVALIDARG);
        }
    }

    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
};

template <typename TIn>
struct Converter<TIn, variant_if_similar_t<TIn, winrt::Windows::Foundation::TimeSpan>>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&)
    {
    }
    VARIANT Convert(const winrt::Windows::Foundation::TimeSpan& value)
    {
        VARIANT result{VT_R8};
        result.dblVal = static_cast<double>(value.count()) / kHundredNanosecondsPerMillisecond;
        return result;
    }
};

template <> struct Converter<VARIANT, winrt::Windows::Foundation::DateTime>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
                  dispatchAdapter)
        : m_dispatchAdapter(dispatchAdapter)
    {
    }
    winrt::Windows::Foundation::DateTime Convert(const VARIANT& value)
    {
        switch (value.vt)
        {
        // We need to include the specific allowed VTs below instead
        // of just relying on the number converter because the default
        // case from the converter returns 0, whereas here we want
        // to throw an invalidarg exception for the default case.
        case VT_I1:
        case VT_UI1:
        case VT_I2:
        case VT_UI2:
        case VT_I4:
        case VT_UI4:
        case VT_I8:
        case VT_UI8:
        case VT_R4:
        case VT_R8:
        case VT_DATE:
        {
            double jsTime = Converter<VARIANT, double>(m_dispatchAdapter).Convert(value);
            int64_t winRTTime = std::llround(JsTimeToWinRTTime(jsTime));
            return winrt::Windows::Foundation::DateTime(
                winrt::Windows::Foundation::TimeSpan(winRTTime));
        }
        default:
            winrt::throw_hresult(E_INVALIDARG);
        }
    }

    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
};

template <typename TIn>
struct Converter<TIn, variant_if_similar_t<TIn, winrt::Windows::Foundation::DateTime>>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&)
    {
    }
    VARIANT Convert(const winrt::Windows::Foundation::DateTime& value)
    {
        VARIANT result{VT_DATE};
        result.date = WinRTTimeToVariantTime(value.time_since_epoch().count());
        return result;
    }
};

template <typename TIn> struct Converter<TIn, variant_if_similar_t<winrt::guid, TIn>>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
                  dispatchAdapter)
        : m_dispatchAdapter(dispatchAdapter)
    {
    }
    VARIANT Convert(const winrt::guid& value)
    {
        // to_hstring will produce a GUID string with {} delimiters
        // which matches the output we want.
        return Converter<winrt::hstring, VARIANT>(m_dispatchAdapter)
            .Convert(winrt::to_hstring(value));
    }
    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
};

// Convert to/from enums. Enums in WinRT are either Int32 or UInt32
template <typename TEnum>
struct Converter<
    TEnum, std::enable_if_t<
               (std::is_enum_v<TEnum> ||
                std::is_enum_v<std::remove_reference_t<TEnum>>)&&std::is_signed_v<TEnum>,
               VARIANT>>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
                  dispatchAdapter)
        : m_dispatchAdapter(dispatchAdapter)
    {
    }
    VARIANT Convert(TEnum value)
    {
        static_assert(sizeof(value) == sizeof(int32_t));
        return Converter<int32_t, VARIANT>(m_dispatchAdapter)
            .Convert(static_cast<int32_t>(value));
    }
    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
};

template <typename TEnum>
struct Converter<
    TEnum, std::enable_if_t<
               (std::is_enum_v<TEnum> ||
                std::is_enum_v<std::remove_reference_t<TEnum>>)&&!std::is_signed_v<TEnum>,
               VARIANT>>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
                  dispatchAdapter)
        : m_dispatchAdapter(dispatchAdapter)
    {
    }
    VARIANT Convert(TEnum value)
    {
        static_assert(sizeof(value) == sizeof(uint32_t));
        return Converter<uint32_t, VARIANT>(m_dispatchAdapter)
            .Convert(static_cast<uint32_t>(value));
    }
    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
};

template <typename TEnum>
struct Converter<
    std::enable_if_t<
        (std::is_enum_v<TEnum> || std::is_enum_v<std::remove_reference_t<TEnum>>), VARIANT>,
    TEnum>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
                  dispatchAdapter)
        : m_dispatchAdapter(dispatchAdapter)
    {
    }
    TEnum Convert(VARIANT value)
    {
        return static_cast<TEnum>(
            Converter<VARIANT, uint32_t>(m_dispatchAdapter).Convert(value));
    }
    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
};

// PropertyValue conversion
template <> struct Converter<VARIANT, winrt::Windows::Foundation::IPropertyValue>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
                  dispatchAdapter)
        : m_dispatchAdapter(dispatchAdapter)
    {
    }
    winrt::Windows::Foundation::IPropertyValue Convert(const VARIANT& value)
    {
        winrt::Windows::Foundation::IInspectable propertyValueAsInspectable{nullptr};
        switch (value.vt)
        {
#define VARIANT_TO_PROPERTY_VALUE_CASE(Variant_VT_TYPE, WinRTType, PropertyValueCreateFn)      \
    case Variant_VT_TYPE:                                                                      \
        propertyValueAsInspectable =                                                           \
            winrt::Windows::Foundation::PropertyValue::PropertyValueCreateFn(                  \
                Converter<VARIANT, WinRTType>(m_dispatchAdapter).Convert(value));              \
        break

            VARIANT_TO_PROPERTY_VALUE_CASE(VT_BOOL, bool, CreateBoolean);
            VARIANT_TO_PROPERTY_VALUE_CASE(VT_BSTR, winrt::hstring, CreateString);
            VARIANT_TO_PROPERTY_VALUE_CASE(VT_I1, byte, CreateUInt8);
            VARIANT_TO_PROPERTY_VALUE_CASE(VT_UI1, byte, CreateUInt8);
            VARIANT_TO_PROPERTY_VALUE_CASE(VT_I2, int16_t, CreateInt16);
            VARIANT_TO_PROPERTY_VALUE_CASE(VT_UI2, uint16_t, CreateUInt16);
            VARIANT_TO_PROPERTY_VALUE_CASE(VT_I4, int32_t, CreateInt32);
            VARIANT_TO_PROPERTY_VALUE_CASE(VT_UI4, uint32_t, CreateUInt32);
            VARIANT_TO_PROPERTY_VALUE_CASE(VT_INT, int32_t, CreateInt32);
            VARIANT_TO_PROPERTY_VALUE_CASE(VT_UINT, uint32_t, CreateUInt32);
            VARIANT_TO_PROPERTY_VALUE_CASE(VT_I8, int64_t, CreateInt64);
            VARIANT_TO_PROPERTY_VALUE_CASE(VT_UI8, uint64_t, CreateUInt64);
            VARIANT_TO_PROPERTY_VALUE_CASE(VT_R4, float, CreateSingle);
            VARIANT_TO_PROPERTY_VALUE_CASE(VT_R8, double, CreateDouble);

            VARIANT_TO_PROPERTY_VALUE_CASE(
                VT_DATE, winrt::Windows::Foundation::DateTime, CreateDateTime);
            VARIANT_TO_PROPERTY_VALUE_CASE(
                VT_FILETIME, winrt::Windows::Foundation::DateTime, CreateDateTime);

        default:
            winrt::hresult_error(E_NOTIMPL);
            break;

        case VT_EMPTY:
        case VT_NULL:
            break;
#undef VARIANT_TO_PROPERTY_VALUE_CASE
        }

        return propertyValueAsInspectable.as<winrt::Windows::Foundation::IPropertyValue>();
    }
    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
};

template <> struct Converter<winrt::Windows::Foundation::IPropertyValue, VARIANT>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
                  dispatchAdapter)
        : m_dispatchAdapter(dispatchAdapter)
    {
    }
    VARIANT Convert(const winrt::Windows::Foundation::IPropertyValue& value)
    {
        switch (value.Type())
        {
#define PROPERTY_VALUE_TO_VARIANT_CASE(PropertyTypeEnum, WinRTType, PropertyTypeMethodName)    \
    case winrt::Windows::Foundation::PropertyType::PropertyTypeEnum:                           \
        return Converter<WinRTType, VARIANT>(m_dispatchAdapter)                                \
            .Convert(value.PropertyTypeMethodName())

        default:
            winrt::hresult_error(E_NOTIMPL);
            break;

        case winrt::Windows::Foundation::PropertyType::Empty:
            return {VT_EMPTY};

            PROPERTY_VALUE_TO_VARIANT_CASE(String, winrt::hstring, GetString);
            PROPERTY_VALUE_TO_VARIANT_CASE(UInt8, uint8_t, GetUInt8);
            PROPERTY_VALUE_TO_VARIANT_CASE(Int16, int16_t, GetInt16);
            PROPERTY_VALUE_TO_VARIANT_CASE(UInt16, uint16_t, GetUInt16);
            PROPERTY_VALUE_TO_VARIANT_CASE(Int32, int32_t, GetInt32);
            PROPERTY_VALUE_TO_VARIANT_CASE(UInt32, uint32_t, GetUInt32);
            PROPERTY_VALUE_TO_VARIANT_CASE(Int64, int64_t, GetInt64);
            PROPERTY_VALUE_TO_VARIANT_CASE(UInt64, uint64_t, GetUInt64);
            PROPERTY_VALUE_TO_VARIANT_CASE(Single, float, GetSingle);
            PROPERTY_VALUE_TO_VARIANT_CASE(Double, double, GetDouble);
            PROPERTY_VALUE_TO_VARIANT_CASE(Char16, int16_t, GetChar16);
            PROPERTY_VALUE_TO_VARIANT_CASE(Boolean, bool, GetBoolean);
            PROPERTY_VALUE_TO_VARIANT_CASE(
                DateTime, winrt::Windows::Foundation::DateTime, GetDateTime);
            PROPERTY_VALUE_TO_VARIANT_CASE(
                TimeSpan, winrt::Windows::Foundation::TimeSpan, GetTimeSpan);
            PROPERTY_VALUE_TO_VARIANT_CASE(Guid, winrt::guid, GetGuid);
#undef PROPERTY_VALUE_TO_VARIANT_CASE
        }
        return {VT_NULL};
    }
    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
};

#pragma warning(4 : 4996)
// WinRT struct to VARIANT Converter.
template <typename StructT>
struct Converter<
    StructT,
    std::enable_if_t<
        std::is_class_v<StructT> && std::is_standard_layout_v<StructT> &&
            !std::is_base_of_v<winrt::Windows::Foundation::IUnknown, StructT> &&
            !std::is_base_of_v<
                winrt::Windows::Foundation::IUnknown, std::remove_reference_t<StructT>> &&
            !TIsVector_v<StructT> && !TIsVectorView_v<StructT> && !TIsArrayView_v<StructT> &&
            !TIsComArray_v<StructT> && !is_similar_v<StructT, winrt::hstring> &&
            !is_similar_v<StructT, winrt::guid> &&
            !is_similar_v<StructT, winrt::Windows::Foundation::TimeSpan> &&
            !is_similar_v<StructT, winrt::Windows::Foundation::DateTime>,
        VARIANT>>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
                  dispatchAdapter)
        : m_dispatchAdapter(dispatchAdapter)
    {
    }
    VARIANT Convert(const StructT& value)
    {
        auto boxed = winrt::box_value(value);
        return Converter<winrt::Windows::Foundation::IInspectable, VARIANT>(m_dispatchAdapter)
            .Convert(boxed);
    }

    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
};

// VARIANT to WinRT struct Converter.
template <typename StructT>
struct Converter<
    std::enable_if_t<
        std::is_class_v<StructT> && std::is_standard_layout_v<StructT> &&
            !std::is_base_of_v<winrt::Windows::Foundation::IUnknown, StructT> &&
            !std::is_base_of_v<
                winrt::Windows::Foundation::IUnknown, std::remove_reference_t<StructT>> &&
            !TIsVector_v<StructT> && !TIsVectorView_v<StructT> && !TIsArrayView_v<StructT> &&
            !TIsComArray_v<StructT> && !is_similar_v<StructT, winrt::hstring> &&
            !is_similar_v<StructT, winrt::guid> &&
            !is_similar_v<StructT, winrt::Windows::Foundation::TimeSpan> &&
            !is_similar_v<StructT, winrt::Windows::Foundation::DateTime>,
        VARIANT>,
    StructT>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
                  dispatchAdapter)
        : m_dispatchAdapter(dispatchAdapter)
    {
    }
    StructT Convert(const VARIANT& valueAsVariant)
    {
        if (valueAsVariant.vt == VT_DISPATCH || valueAsVariant.vt == VT_UNKNOWN)
        {
            winrt::com_ptr<ICoreWebView2PrivateRemoteDictionary> valueAsRemoteDictionary;
            valueAsVariant.pdispVal->QueryInterface(
                IID_PPV_ARGS(valueAsRemoteDictionary.put()));
            if (valueAsRemoteDictionary)
            {
                return Converter<ICoreWebView2PrivateRemoteDictionary*, StructT>(
                           m_dispatchAdapter)
                    .Convert(valueAsRemoteDictionary.get());
            }
        }

        auto boxed =
            Converter<VARIANT, winrt::Windows::Foundation::IInspectable>(m_dispatchAdapter)
                .Convert(valueAsVariant);
        return winrt::unbox_value<StructT>(boxed);
    }

    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
};

template <typename RefT> struct Converter<VARIANT, winrt::Windows::Foundation::IReference<RefT>>
{
    Converter(const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter&
                  dispatchAdapter)
        : m_dispatchAdapter(dispatchAdapter)
    {
    }
    winrt::Windows::Foundation::IReference<RefT> Convert(const VARIANT& valueAsVariant)
    {
        if (valueAsVariant.vt == VT_NULL || valueAsVariant.vt == VT_EMPTY)
        {
            return {nullptr};
        }
        else
        {
            return {Converter<VARIANT, RefT>(m_dispatchAdapter).Convert(valueAsVariant)};
        }
    }

    winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter m_dispatchAdapter;
};
} // namespace wv2winrt_impl

