// Copyright (C) Microsoft Corporation. All rights reserved.
// Use of this source code is governed by a BSD-style license that can be
// found in the LICENSE file.
#pragma once
#include <windows.h>

namespace wv2winrt_impl
{

interface __declspec(uuid("F2A70B4A-FA05-45DA-9A4E-7BE0467E18E7"))
    ICoreWebView2PrivateRemoteDictionary : public IUnknown
{
    virtual HRESULT STDMETHODCALLTYPE GetValue(void** result) = 0;
};

} // namespace wv2winrt_impl

