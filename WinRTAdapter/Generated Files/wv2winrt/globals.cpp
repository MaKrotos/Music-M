// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#include "pch.h"

#include "wv2winrt/globals.h"
#include "wv2winrt/NativeCode.g.h"
#include "wv2winrt/Windows.g.h"
#include "wv2winrt/Windows.Globalization.g.h"
#include "wv2winrt/Windows.System.g.h"
#include "wv2winrt/Windows.Foundation.g.h"
#include "wv2winrt/Windows.Storage.g.h"
#include "wv2winrt/Windows.Data.g.h"
#include "wv2winrt/Windows.System.UserProfile.g.h"
#include "wv2winrt/Windows.Foundation.Collections.g.h"
#include "wv2winrt/Windows.Storage.FileProperties.g.h"
#include "wv2winrt/Windows.Storage.Streams.g.h"
#include "wv2winrt/Windows.Storage.Search.g.h"
#include "wv2winrt/Windows.Data.Text.g.h"

namespace wv2winrt_impl
{
    extern const GlobalEntry s_globalEntries[] =
    {
        {
            L"NativeCode",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Namespace_NativeCode>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Namespace_Windows>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Globalization",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Namespace_Windows_Globalization>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.System",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Namespace_Windows_System>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Foundation",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Namespace_Windows_Foundation>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Namespace_Windows_Storage>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Data",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Namespace_Windows_Data>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.System.UserProfile",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Namespace_Windows_System_UserProfile>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Foundation.Collections",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Namespace_Windows_Foundation_Collections>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.FileProperties",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Namespace_Windows_Storage_FileProperties>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.Streams",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Namespace_Windows_Storage_Streams>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.Search",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Namespace_Windows_Storage_Search>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Data.Text",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Namespace_Windows_Data_Text>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Globalization.LanguageLayoutDirection",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Globalization_LanguageLayoutDirection>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Globalization.DayOfWeek",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Globalization_DayOfWeek>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.System.UserPictureSize",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_System_UserPictureSize>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.System.UserAgeConsentGroup",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_System_UserAgeConsentGroup>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.System.UserType",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_System_UserType>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.System.UserAuthenticationStatus",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_System_UserAuthenticationStatus>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.System.UserAgeConsentResult",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_System_UserAgeConsentResult>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.System.UserWatcherStatus",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_System_UserWatcherStatus>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.System.UserWatcherUpdateKind",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_System_UserWatcherUpdateKind>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Foundation.AsyncStatus",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Foundation_AsyncStatus>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Foundation.PropertyType",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Foundation_PropertyType>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.FileAccessMode",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Storage_FileAccessMode>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.NameCollisionOption",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Storage_NameCollisionOption>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.StorageDeleteOption",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Storage_StorageDeleteOption>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.StorageItemTypes",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Storage_StorageItemTypes>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.StorageOpenOptions",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Storage_StorageOpenOptions>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.FileAttributes",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Storage_FileAttributes>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.CreationCollisionOption",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Storage_CreationCollisionOption>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.StorageLibraryChangeType",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Storage_StorageLibraryChangeType>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.FileProperties.ThumbnailMode",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Storage_FileProperties_ThumbnailMode>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.FileProperties.ThumbnailOptions",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Storage_FileProperties_ThumbnailOptions>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.FileProperties.ThumbnailType",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Storage_FileProperties_ThumbnailType>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.FileProperties.PropertyPrefetchOptions",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Storage_FileProperties_PropertyPrefetchOptions>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.FileProperties.VideoOrientation",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Storage_FileProperties_VideoOrientation>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.FileProperties.PhotoOrientation",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Storage_FileProperties_PhotoOrientation>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.Streams.InputStreamOptions",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Storage_Streams_InputStreamOptions>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.Search.CommonFileQuery",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Storage_Search_CommonFileQuery>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.Search.CommonFolderQuery",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Storage_Search_CommonFolderQuery>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.Search.IndexedState",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Storage_Search_IndexedState>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.Search.IndexerOption",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Storage_Search_IndexerOption>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.Search.FolderDepth",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Storage_Search_FolderDepth>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.Search.DateStackOption",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Enum_Windows_Storage_Search_DateStackOption>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"NativeCode.PlayWithNumbers",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Static_Class_NativeCode_PlayWithNumbers>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Globalization.Language",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Static_Class_Windows_Globalization_Language>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.System.User",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Static_Class_Windows_System_User>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Foundation.Uri",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Static_Class_Windows_Foundation_Uri>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Foundation.WwwFormUrlDecoder",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Static_Class_Windows_Foundation_WwwFormUrlDecoder>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.StorageFile",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Static_Class_Windows_Storage_StorageFile>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.StorageFolder",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Static_Class_Windows_Storage_StorageFolder>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.StorageLibraryChangeTrackerOptions",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Static_Class_Windows_Storage_StorageLibraryChangeTrackerOptions>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.System.UserProfile.AdvertisingManager",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Static_Class_Windows_System_UserProfile_AdvertisingManager>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.System.UserProfile.AssignedAccessSettings",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Static_Class_Windows_System_UserProfile_AssignedAccessSettings>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.System.UserProfile.DiagnosticsSettings",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Static_Class_Windows_System_UserProfile_DiagnosticsSettings>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.System.UserProfile.FirstSignInSettings",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Static_Class_Windows_System_UserProfile_FirstSignInSettings>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.System.UserProfile.GlobalizationPreferences",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Static_Class_Windows_System_UserProfile_GlobalizationPreferences>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.System.UserProfile.UserProfilePersonalizationSettings",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Static_Class_Windows_System_UserProfile_UserProfilePersonalizationSettings>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        },
        {
            L"Windows.Storage.Search.QueryOptions",
            ([](const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter, IDispatch** dispatch) -> HRESULT
                {
                    winrt::com_ptr<IDispatch> obj = winrt::make<Static_Class_Windows_Storage_Search_QueryOptions>(dispatchAdapter);
                    *dispatch = obj.detach();
                    return S_OK;
                }
            )
        }
    };
    extern const size_t s_globalEntriesCount = ARRAYSIZE(s_globalEntries);

    extern const InstanceConstructibleEntry s_instanceConstructibleEntries[] =
    {
        {
            L"NativeCode.IPlayWithNumbers",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_NativeCode_IPlayWithNumbers>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"NativeCode.PlayWithNumbers",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_NativeCode_PlayWithNumbers>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncActionCompletedHandler",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncActionCompletedHandler>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Boolean>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_bool_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Object>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_IInspectable_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<UInt32>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_uint32_t_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Windows.Foundation.Collections.IMap`2<String, Object>>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_Windows_Foundation_Collections_IMap_2_HSTRING_IInspectable__>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Windows.Foundation.Collections.IPropertySet>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_Windows_Foundation_Collections_IPropertySet_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Windows.Foundation.Collections.IVectorView`1<Windows.Storage.IStorageItem>>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_Windows_Foundation_Collections_IVectorView_1_Windows_Storage_IStorageItem__>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Windows.Foundation.Collections.IVectorView`1<Windows.Storage.StorageFile>>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_Windows_Foundation_Collections_IVectorView_1_Windows_Storage_StorageFile__>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Windows.Foundation.Collections.IVectorView`1<Windows.Storage.StorageFolder>>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_Windows_Foundation_Collections_IVectorView_1_Windows_Storage_StorageFolder__>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Windows.Foundation.Collections.IVectorView`1<Windows.Storage.StorageLibraryChange>>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_Windows_Foundation_Collections_IVectorView_1_Windows_Storage_StorageLibraryChange__>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Windows.Foundation.Collections.IVectorView`1<Windows.System.User>>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_Windows_Foundation_Collections_IVectorView_1_Windows_System_User__>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Windows.Storage.FileProperties.BasicProperties>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_Windows_Storage_FileProperties_BasicProperties_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Windows.Storage.FileProperties.DocumentProperties>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_Windows_Storage_FileProperties_DocumentProperties_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Windows.Storage.FileProperties.ImageProperties>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_Windows_Storage_FileProperties_ImageProperties_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Windows.Storage.FileProperties.MusicProperties>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_Windows_Storage_FileProperties_MusicProperties_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Windows.Storage.FileProperties.StorageItemThumbnail>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_Windows_Storage_FileProperties_StorageItemThumbnail_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Windows.Storage.FileProperties.VideoProperties>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_Windows_Storage_FileProperties_VideoProperties_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Windows.Storage.IStorageItem>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_Windows_Storage_IStorageItem_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Windows.Storage.Search.IndexedState>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_Windows_Storage_Search_IndexedState_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Windows.Storage.StorageFile>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_Windows_Storage_StorageFile_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Windows.Storage.StorageFolder>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_Windows_Storage_StorageFolder_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Windows.Storage.StorageStreamTransaction>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_Windows_Storage_StorageStreamTransaction_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Windows.Storage.Streams.IInputStream>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_Windows_Storage_Streams_IInputStream_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Windows.Storage.Streams.IRandomAccessStream>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_Windows_Storage_Streams_IRandomAccessStream_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Windows.Storage.Streams.IRandomAccessStreamReference>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_Windows_Storage_Streams_IRandomAccessStreamReference_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Windows.Storage.Streams.IRandomAccessStreamWithContentType>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_Windows_Storage_Streams_IRandomAccessStreamWithContentType_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationCompletedHandler`1<Windows.System.UserAgeConsentResult>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationCompletedHandler_1_Windows_System_UserAgeConsentResult_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationProgressHandler`2<UInt32, UInt32>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationProgressHandler_2_uint32_t_uint32_t_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationProgressHandler`2<Windows.Storage.Streams.IBuffer, UInt32>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationProgressHandler_2_Windows_Storage_Streams_IBuffer_uint32_t_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationWithProgressCompletedHandler`2<UInt32, UInt32>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationWithProgressCompletedHandler_2_uint32_t_uint32_t_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.AsyncOperationWithProgressCompletedHandler`2<Windows.Storage.Streams.IBuffer, UInt32>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_AsyncOperationWithProgressCompletedHandler_2_Windows_Storage_Streams_IBuffer_uint32_t_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IIterable`1<String>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IIterable_1_HSTRING_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IIterable`1<Windows.Data.Text.TextSegment>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IIterable_1_Windows_Data_Text_TextSegment_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IIterable`1<Windows.Foundation.Collections.IKeyValuePair`2<String, Object>>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IIterable_1_Windows_Foundation_Collections_IKeyValuePair_2_HSTRING_IInspectable__>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IIterable`1<Windows.Foundation.Collections.IKeyValuePair`2<String, Windows.Foundation.Collections.IVectorView`1<Windows.Data.Text.TextSegment>>>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IIterable_1_Windows_Foundation_Collections_IKeyValuePair_2_HSTRING_Windows_Foundation_Collections_IVectorView_1_Windows_Data_Text_TextSegment___>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IIterable`1<Windows.Foundation.IWwwFormUrlDecoderEntry>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IIterable_1_Windows_Foundation_IWwwFormUrlDecoderEntry_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IIterable`1<Windows.Storage.IStorageItem>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IIterable_1_Windows_Storage_IStorageItem_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IIterable`1<Windows.Storage.Search.SortEntry>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IIterable_1_Windows_Storage_Search_SortEntry_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IIterable`1<Windows.Storage.StorageFile>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IIterable_1_Windows_Storage_StorageFile_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IIterable`1<Windows.Storage.StorageFolder>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IIterable_1_Windows_Storage_StorageFolder_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IIterable`1<Windows.Storage.StorageLibraryChange>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IIterable_1_Windows_Storage_StorageLibraryChange_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IIterable`1<Windows.System.User>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IIterable_1_Windows_System_User_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IIterable`1<Windows.System.UserWatcherUpdateKind>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IIterable_1_Windows_System_UserWatcherUpdateKind_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IIterator`1<String>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IIterator_1_HSTRING_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IIterator`1<Windows.Data.Text.TextSegment>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IIterator_1_Windows_Data_Text_TextSegment_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IIterator`1<Windows.Foundation.Collections.IKeyValuePair`2<String, Object>>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IIterator_1_Windows_Foundation_Collections_IKeyValuePair_2_HSTRING_IInspectable__>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IIterator`1<Windows.Foundation.Collections.IKeyValuePair`2<String, Windows.Foundation.Collections.IVectorView`1<Windows.Data.Text.TextSegment>>>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IIterator_1_Windows_Foundation_Collections_IKeyValuePair_2_HSTRING_Windows_Foundation_Collections_IVectorView_1_Windows_Data_Text_TextSegment___>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IIterator`1<Windows.Foundation.IWwwFormUrlDecoderEntry>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IIterator_1_Windows_Foundation_IWwwFormUrlDecoderEntry_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IIterator`1<Windows.Storage.IStorageItem>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IIterator_1_Windows_Storage_IStorageItem_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IIterator`1<Windows.Storage.Search.SortEntry>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IIterator_1_Windows_Storage_Search_SortEntry_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IIterator`1<Windows.Storage.StorageFile>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IIterator_1_Windows_Storage_StorageFile_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IIterator`1<Windows.Storage.StorageFolder>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IIterator_1_Windows_Storage_StorageFolder_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IIterator`1<Windows.Storage.StorageLibraryChange>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IIterator_1_Windows_Storage_StorageLibraryChange_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IIterator`1<Windows.System.User>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IIterator_1_Windows_System_User_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IIterator`1<Windows.System.UserWatcherUpdateKind>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IIterator_1_Windows_System_UserWatcherUpdateKind_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IKeyValuePair`2<String, Object>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IKeyValuePair_2_HSTRING_IInspectable_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IKeyValuePair`2<String, Windows.Foundation.Collections.IVectorView`1<Windows.Data.Text.TextSegment>>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IKeyValuePair_2_HSTRING_Windows_Foundation_Collections_IVectorView_1_Windows_Data_Text_TextSegment__>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IMapView`2<String, Object>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IMapView_2_HSTRING_IInspectable_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IMapView`2<String, Windows.Foundation.Collections.IVectorView`1<Windows.Data.Text.TextSegment>>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IMapView_2_HSTRING_Windows_Foundation_Collections_IVectorView_1_Windows_Data_Text_TextSegment__>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IMap`2<String, Object>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IMap_2_HSTRING_IInspectable_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IMap`2<String, Windows.Foundation.Collections.IVectorView`1<Windows.Data.Text.TextSegment>>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IMap_2_HSTRING_Windows_Foundation_Collections_IVectorView_1_Windows_Data_Text_TextSegment__>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IObservableMap`2<String, Object>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IObservableMap_2_HSTRING_IInspectable_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IPropertySet",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IPropertySet>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IVectorView`1<String>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IVectorView_1_HSTRING_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IVectorView`1<Windows.Data.Text.TextSegment>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IVectorView_1_Windows_Data_Text_TextSegment_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IVectorView`1<Windows.Foundation.IWwwFormUrlDecoderEntry>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IVectorView_1_Windows_Foundation_IWwwFormUrlDecoderEntry_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IVectorView`1<Windows.Storage.IStorageItem>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IVectorView_1_Windows_Storage_IStorageItem_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IVectorView`1<Windows.Storage.Search.SortEntry>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IVectorView_1_Windows_Storage_Search_SortEntry_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IVectorView`1<Windows.Storage.StorageFile>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IVectorView_1_Windows_Storage_StorageFile_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IVectorView`1<Windows.Storage.StorageFolder>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IVectorView_1_Windows_Storage_StorageFolder_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IVectorView`1<Windows.Storage.StorageLibraryChange>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IVectorView_1_Windows_Storage_StorageLibraryChange_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IVectorView`1<Windows.System.User>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IVectorView_1_Windows_System_User_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IVectorView`1<Windows.System.UserWatcherUpdateKind>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IVectorView_1_Windows_System_UserWatcherUpdateKind_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IVector`1<String>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IVector_1_HSTRING_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.IVector`1<Windows.Storage.Search.SortEntry>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_IVector_1_Windows_Storage_Search_SortEntry_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Collections.MapChangedEventHandler`2<String, Object>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Collections_MapChangedEventHandler_2_HSTRING_IInspectable_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncAction",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncAction>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncInfo",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncInfo>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperationWithProgress`2<UInt32, UInt32>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperationWithProgress_2_uint32_t_uint32_t_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperationWithProgress`2<Windows.Storage.Streams.IBuffer, UInt32>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperationWithProgress_2_Windows_Storage_Streams_IBuffer_uint32_t_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Boolean>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_bool_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Object>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_IInspectable_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<UInt32>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_uint32_t_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Windows.Foundation.Collections.IMap`2<String, Object>>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_Windows_Foundation_Collections_IMap_2_HSTRING_IInspectable__>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Windows.Foundation.Collections.IPropertySet>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_Windows_Foundation_Collections_IPropertySet_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Windows.Foundation.Collections.IVectorView`1<Windows.Storage.IStorageItem>>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_Windows_Foundation_Collections_IVectorView_1_Windows_Storage_IStorageItem__>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Windows.Foundation.Collections.IVectorView`1<Windows.Storage.StorageFile>>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_Windows_Foundation_Collections_IVectorView_1_Windows_Storage_StorageFile__>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Windows.Foundation.Collections.IVectorView`1<Windows.Storage.StorageFolder>>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_Windows_Foundation_Collections_IVectorView_1_Windows_Storage_StorageFolder__>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Windows.Foundation.Collections.IVectorView`1<Windows.Storage.StorageLibraryChange>>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_Windows_Foundation_Collections_IVectorView_1_Windows_Storage_StorageLibraryChange__>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Windows.Foundation.Collections.IVectorView`1<Windows.System.User>>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_Windows_Foundation_Collections_IVectorView_1_Windows_System_User__>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Windows.Storage.FileProperties.BasicProperties>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_Windows_Storage_FileProperties_BasicProperties_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Windows.Storage.FileProperties.DocumentProperties>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_Windows_Storage_FileProperties_DocumentProperties_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Windows.Storage.FileProperties.ImageProperties>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_Windows_Storage_FileProperties_ImageProperties_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Windows.Storage.FileProperties.MusicProperties>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_Windows_Storage_FileProperties_MusicProperties_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Windows.Storage.FileProperties.StorageItemThumbnail>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_Windows_Storage_FileProperties_StorageItemThumbnail_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Windows.Storage.FileProperties.VideoProperties>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_Windows_Storage_FileProperties_VideoProperties_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Windows.Storage.IStorageItem>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_Windows_Storage_IStorageItem_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Windows.Storage.Search.IndexedState>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_Windows_Storage_Search_IndexedState_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Windows.Storage.StorageFile>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_Windows_Storage_StorageFile_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Windows.Storage.StorageFolder>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_Windows_Storage_StorageFolder_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Windows.Storage.StorageStreamTransaction>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_Windows_Storage_StorageStreamTransaction_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Windows.Storage.Streams.IInputStream>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_Windows_Storage_Streams_IInputStream_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Windows.Storage.Streams.IRandomAccessStream>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_Windows_Storage_Streams_IRandomAccessStream_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Windows.Storage.Streams.IRandomAccessStreamReference>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_Windows_Storage_Streams_IRandomAccessStreamReference_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Windows.Storage.Streams.IRandomAccessStreamWithContentType>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_Windows_Storage_Streams_IRandomAccessStreamWithContentType_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IAsyncOperation`1<Windows.System.UserAgeConsentResult>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IAsyncOperation_1_Windows_System_UserAgeConsentResult_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IClosable",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IClosable>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IPropertyValue",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IPropertyValue>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IReference`1<Double>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IReference_1_double_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IReference`1<Windows.Data.Text.TextSegment>",
            true,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    winrt::Windows::Foundation::IInspectable inspectable;
                    winrt::copy_from_abi(inspectable, abiInspectable);
                    return winrt::make<Struct_Windows_Data_Text_TextSegment>(winrt::unbox_value<winrt::Windows::Data::Text::TextSegment>(inspectable), dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IReference`1<Windows.Foundation.DateTime>",
            true,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    winrt::Windows::Foundation::IInspectable inspectable;
                    winrt::copy_from_abi(inspectable, abiInspectable);
                    return winrt::make<Struct_Windows_Foundation_DateTime>(winrt::unbox_value<winrt::Windows::Foundation::DateTime>(inspectable), dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IReference`1<Windows.Foundation.HResult>",
            true,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    winrt::Windows::Foundation::IInspectable inspectable;
                    winrt::copy_from_abi(inspectable, abiInspectable);
                    return winrt::make<Struct_Windows_Foundation_HResult>(winrt::unbox_value<winrt::hresult>(inspectable), dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IReference`1<Windows.Foundation.Point>",
            true,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    winrt::Windows::Foundation::IInspectable inspectable;
                    winrt::copy_from_abi(inspectable, abiInspectable);
                    return winrt::make<Struct_Windows_Foundation_Point>(winrt::unbox_value<winrt::Windows::Foundation::Point>(inspectable), dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IReference`1<Windows.Foundation.Rect>",
            true,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    winrt::Windows::Foundation::IInspectable inspectable;
                    winrt::copy_from_abi(inspectable, abiInspectable);
                    return winrt::make<Struct_Windows_Foundation_Rect>(winrt::unbox_value<winrt::Windows::Foundation::Rect>(inspectable), dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IReference`1<Windows.Foundation.Size>",
            true,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    winrt::Windows::Foundation::IInspectable inspectable;
                    winrt::copy_from_abi(inspectable, abiInspectable);
                    return winrt::make<Struct_Windows_Foundation_Size>(winrt::unbox_value<winrt::Windows::Foundation::Size>(inspectable), dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IReference`1<Windows.Foundation.TimeSpan>",
            true,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    winrt::Windows::Foundation::IInspectable inspectable;
                    winrt::copy_from_abi(inspectable, abiInspectable);
                    return winrt::make<Struct_Windows_Foundation_TimeSpan>(winrt::unbox_value<winrt::Windows::Foundation::TimeSpan>(inspectable), dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IReference`1<Windows.Storage.Search.SortEntry>",
            true,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    winrt::Windows::Foundation::IInspectable inspectable;
                    winrt::copy_from_abi(inspectable, abiInspectable);
                    return winrt::make<Struct_Windows_Storage_Search_SortEntry>(winrt::unbox_value<winrt::Windows::Storage::Search::SortEntry>(inspectable), dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IStringable",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IStringable>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IUriRuntimeClass",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IUriRuntimeClass>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IUriRuntimeClassFactory",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IUriRuntimeClassFactory>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IUriRuntimeClassWithAbsoluteCanonicalUri",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IUriRuntimeClassWithAbsoluteCanonicalUri>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IWwwFormUrlDecoderEntry",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IWwwFormUrlDecoderEntry>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IWwwFormUrlDecoderRuntimeClass",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IWwwFormUrlDecoderRuntimeClass>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.IWwwFormUrlDecoderRuntimeClassFactory",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_IWwwFormUrlDecoderRuntimeClassFactory>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.TypedEventHandler`2<Windows.Storage.Search.IStorageQueryResultBase, Object>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_TypedEventHandler_2_Windows_Storage_Search_IStorageQueryResultBase_IInspectable_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.TypedEventHandler`2<Windows.System.UserWatcher, Object>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_TypedEventHandler_2_Windows_System_UserWatcher_IInspectable_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.TypedEventHandler`2<Windows.System.UserWatcher, Windows.System.UserAuthenticationStatusChangingEventArgs>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_TypedEventHandler_2_Windows_System_UserWatcher_Windows_System_UserAuthenticationStatusChangingEventArgs_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.TypedEventHandler`2<Windows.System.UserWatcher, Windows.System.UserChangedEventArgs>",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_TypedEventHandler_2_Windows_System_UserWatcher_Windows_System_UserChangedEventArgs_>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.Uri",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_Uri>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Foundation.WwwFormUrlDecoder",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Foundation_WwwFormUrlDecoder>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Globalization.ILanguage",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Globalization_ILanguage>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Globalization.ILanguage2",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Globalization_ILanguage2>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Globalization.ILanguage3",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Globalization_ILanguage3>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Globalization.ILanguageExtensionSubtags",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Globalization_ILanguageExtensionSubtags>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Globalization.ILanguageFactory",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Globalization_ILanguageFactory>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Globalization.ILanguageStatics3",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Globalization_ILanguageStatics3>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Globalization.Language",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Globalization_Language>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.FileProperties.BasicProperties",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_FileProperties_BasicProperties>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.FileProperties.DocumentProperties",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_FileProperties_DocumentProperties>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.FileProperties.IBasicProperties",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_FileProperties_IBasicProperties>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.FileProperties.IDocumentProperties",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_FileProperties_IDocumentProperties>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.FileProperties.IImageProperties",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_FileProperties_IImageProperties>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.FileProperties.IMusicProperties",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_FileProperties_IMusicProperties>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.FileProperties.IStorageItemContentProperties",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_FileProperties_IStorageItemContentProperties>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.FileProperties.IStorageItemExtraProperties",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_FileProperties_IStorageItemExtraProperties>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.FileProperties.IThumbnailProperties",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_FileProperties_IThumbnailProperties>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.FileProperties.IVideoProperties",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_FileProperties_IVideoProperties>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.FileProperties.ImageProperties",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_FileProperties_ImageProperties>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.FileProperties.MusicProperties",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_FileProperties_MusicProperties>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.FileProperties.StorageItemContentProperties",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_FileProperties_StorageItemContentProperties>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.FileProperties.StorageItemThumbnail",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_FileProperties_StorageItemThumbnail>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.FileProperties.VideoProperties",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_FileProperties_VideoProperties>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.IStorageFile",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_IStorageFile>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.IStorageFile2",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_IStorageFile2>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.IStorageFilePropertiesWithAvailability",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_IStorageFilePropertiesWithAvailability>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.IStorageFileStatics",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_IStorageFileStatics>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.IStorageFolder",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_IStorageFolder>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.IStorageFolder2",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_IStorageFolder2>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.IStorageFolder3",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_IStorageFolder3>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.IStorageItem",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_IStorageItem>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.IStorageItem2",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_IStorageItem2>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.IStorageItemProperties",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_IStorageItemProperties>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.IStorageItemProperties2",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_IStorageItemProperties2>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.IStorageItemPropertiesWithProvider",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_IStorageItemPropertiesWithProvider>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.IStorageLibraryChange",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_IStorageLibraryChange>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.IStorageLibraryChangeReader",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_IStorageLibraryChangeReader>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.IStorageLibraryChangeReader2",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_IStorageLibraryChangeReader2>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.IStorageLibraryChangeTracker",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_IStorageLibraryChangeTracker>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.IStorageLibraryChangeTracker2",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_IStorageLibraryChangeTracker2>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.IStorageLibraryChangeTrackerOptions",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_IStorageLibraryChangeTrackerOptions>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.IStorageProvider",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_IStorageProvider>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.IStorageProvider2",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_IStorageProvider2>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.IStorageStreamTransaction",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_IStorageStreamTransaction>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.Search.IQueryOptions",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_Search_IQueryOptions>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.Search.IQueryOptionsFactory",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_Search_IQueryOptionsFactory>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.Search.IQueryOptionsWithProviderFilter",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_Search_IQueryOptionsWithProviderFilter>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.Search.IStorageFileQueryResult",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_Search_IStorageFileQueryResult>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.Search.IStorageFileQueryResult2",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_Search_IStorageFileQueryResult2>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.Search.IStorageFolderQueryOperations",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_Search_IStorageFolderQueryOperations>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.Search.IStorageFolderQueryResult",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_Search_IStorageFolderQueryResult>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.Search.IStorageItemQueryResult",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_Search_IStorageItemQueryResult>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.Search.IStorageQueryResultBase",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_Search_IStorageQueryResultBase>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.Search.QueryOptions",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_Search_QueryOptions>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.Search.StorageFileQueryResult",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_Search_StorageFileQueryResult>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.Search.StorageFolderQueryResult",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_Search_StorageFolderQueryResult>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.Search.StorageItemQueryResult",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_Search_StorageItemQueryResult>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.StorageFile",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_StorageFile>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.StorageFolder",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_StorageFolder>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.StorageLibraryChange",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_StorageLibraryChange>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.StorageLibraryChangeReader",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_StorageLibraryChangeReader>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.StorageLibraryChangeTracker",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_StorageLibraryChangeTracker>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.StorageLibraryChangeTrackerOptions",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_StorageLibraryChangeTrackerOptions>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.StorageProvider",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_StorageProvider>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.StorageStreamTransaction",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_StorageStreamTransaction>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.StreamedFileDataRequestedHandler",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_StreamedFileDataRequestedHandler>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.Streams.IBuffer",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_Streams_IBuffer>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.Streams.IContentTypeProvider",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_Streams_IContentTypeProvider>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.Streams.IInputStream",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_Streams_IInputStream>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.Streams.IInputStreamReference",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_Streams_IInputStreamReference>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.Streams.IOutputStream",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_Streams_IOutputStream>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.Streams.IRandomAccessStream",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_Streams_IRandomAccessStream>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.Streams.IRandomAccessStreamReference",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_Streams_IRandomAccessStreamReference>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.Storage.Streams.IRandomAccessStreamWithContentType",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_Storage_Streams_IRandomAccessStreamWithContentType>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.IUser",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_IUser>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.IUser2",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_IUser2>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.IUserAuthenticationStatusChangeDeferral",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_IUserAuthenticationStatusChangeDeferral>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.IUserAuthenticationStatusChangingEventArgs",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_IUserAuthenticationStatusChangingEventArgs>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.IUserChangedEventArgs",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_IUserChangedEventArgs>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.IUserChangedEventArgs2",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_IUserChangedEventArgs2>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.IUserStatics",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_IUserStatics>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.IUserWatcher",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_IUserWatcher>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.User",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_User>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserAuthenticationStatusChangeDeferral",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserAuthenticationStatusChangeDeferral>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserAuthenticationStatusChangingEventArgs",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserAuthenticationStatusChangingEventArgs>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserChangedEventArgs",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserChangedEventArgs>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserProfile.AdvertisingManager",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserProfile_AdvertisingManager>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserProfile.AdvertisingManagerForUser",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserProfile_AdvertisingManagerForUser>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserProfile.AssignedAccessSettings",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserProfile_AssignedAccessSettings>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserProfile.DiagnosticsSettings",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserProfile_DiagnosticsSettings>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserProfile.FirstSignInSettings",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserProfile_FirstSignInSettings>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserProfile.GlobalizationPreferences",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserProfile_GlobalizationPreferences>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserProfile.GlobalizationPreferencesForUser",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserProfile_GlobalizationPreferencesForUser>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserProfile.IAdvertisingManagerForUser",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserProfile_IAdvertisingManagerForUser>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserProfile.IAdvertisingManagerStatics",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserProfile_IAdvertisingManagerStatics>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserProfile.IAdvertisingManagerStatics2",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserProfile_IAdvertisingManagerStatics2>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserProfile.IAssignedAccessSettings",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserProfile_IAssignedAccessSettings>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserProfile.IAssignedAccessSettingsStatics",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserProfile_IAssignedAccessSettingsStatics>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserProfile.IDiagnosticsSettings",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserProfile_IDiagnosticsSettings>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserProfile.IDiagnosticsSettingsStatics",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserProfile_IDiagnosticsSettingsStatics>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserProfile.IFirstSignInSettings",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserProfile_IFirstSignInSettings>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserProfile.IFirstSignInSettingsStatics",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserProfile_IFirstSignInSettingsStatics>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserProfile.IGlobalizationPreferencesForUser",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserProfile_IGlobalizationPreferencesForUser>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserProfile.IGlobalizationPreferencesStatics",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserProfile_IGlobalizationPreferencesStatics>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserProfile.IGlobalizationPreferencesStatics2",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserProfile_IGlobalizationPreferencesStatics2>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserProfile.IGlobalizationPreferencesStatics3",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserProfile_IGlobalizationPreferencesStatics3>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserProfile.IUserProfilePersonalizationSettings",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserProfile_IUserProfilePersonalizationSettings>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserProfile.IUserProfilePersonalizationSettingsStatics",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserProfile_IUserProfilePersonalizationSettingsStatics>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserProfile.UserProfilePersonalizationSettings",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserProfile_UserProfilePersonalizationSettings>(abiInspectable, dispatchAdapter);
                }
            )
        },
        {
            L"Windows.System.UserWatcher",
            false,
            ([](IInspectable* abiInspectable,
                    const winrt::Microsoft::Web::WebView2::Core::ICoreWebView2DispatchAdapter& dispatchAdapter) -> winrt::com_ptr<IDispatch>
                {
                    return winrt::make<Class_Windows_System_UserWatcher>(abiInspectable, dispatchAdapter);
                }
            )
        }
    };
    extern const size_t s_instanceConstructibleEntriesCount = 241;
}
