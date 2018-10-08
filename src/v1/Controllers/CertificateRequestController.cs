﻿// ------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.IIoT.OpcUa.Services.Vault.v1.Auth;
using Microsoft.Azure.IIoT.OpcUa.Services.Vault.v1.Filters;
using Microsoft.Azure.IIoT.OpcUa.Services.Vault.v1.Models;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace Microsoft.Azure.IIoT.OpcUa.Services.Vault.v1.Controllers
{
    /// <inheritdoc/>
    [Route(VersionInfo.PATH + "/request"), TypeFilter(typeof(ExceptionsFilterAttribute))]
    [Produces("application/json")]
    [Authorize(Policy = Policies.CanRead)]

    public sealed class CertificateRequestController : Controller
    {
        private readonly ICertificateRequest _certificateRequest;

        /// <inheritdoc/>
        public CertificateRequestController(
            ICertificateRequest certificateRequest)
        {
            _certificateRequest = certificateRequest;
        }

        /// <summary>
        /// Start a new signing request.
        /// </summary>
        [HttpPost("sign")]
        [SwaggerOperation(OperationId = "StartSigningRequest")]
        [Authorize(Policy = Policies.CanWrite)]
        public async Task<string> StartSigningRequestAsync([FromBody] StartSigningRequestApiModel signingRequest)
        {
            if (signingRequest == null)
            {
                throw new ArgumentNullException(nameof(signingRequest));
            }
            return await this._certificateRequest.StartSigningRequestAsync(
                signingRequest.ApplicationId,
                signingRequest.CertificateGroupId,
                signingRequest.CertificateTypeId,
                signingRequest.ToServiceModel(),
                signingRequest.AuthorityId);
        }

        /// <summary>
        /// Start a new key pair request.
        /// </summary>
        [HttpPost("newkeypair")]
        [SwaggerOperation(OperationId = "StartNewKeyPairRequest")]
        [Authorize(Policy = Policies.CanWrite)]
        public async Task<string> StartNewKeyPairRequestAsync([FromBody] StartNewKeyPairRequestApiModel newKeyPairRequest)
        {
            if (newKeyPairRequest == null)
            {
                throw new ArgumentNullException(nameof(newKeyPairRequest));
            }
            return await _certificateRequest.StartNewKeyPairRequestAsync(
                newKeyPairRequest.ApplicationId,
                newKeyPairRequest.CertificateGroupId,
                newKeyPairRequest.CertificateTypeId,
                newKeyPairRequest.SubjectName,
                newKeyPairRequest.DomainNames,
                newKeyPairRequest.PrivateKeyFormat,
                newKeyPairRequest.PrivateKeyPassword,
                newKeyPairRequest.AuthorityId);
        }

        /// <summary>
        /// Approve request.
        /// </summary>
        [HttpPost("{requestId}/approve/{rejected}")]
        [SwaggerOperation(OperationId = "ApproveCertificateRequest")]
        [Authorize(Policy = Policies.CanManage)]

        public async Task ApproveCertificateRequestAsync(string requestId, bool rejected)
        {
            var onBehalfOfCertificateRequest = await this._certificateRequest.OnBehalfOfRequest(Request);
            await onBehalfOfCertificateRequest.ApproveAsync(requestId, rejected);
        }

        /// <summary>
        /// Accept request.
        /// </summary>
        [HttpPost("{requestId}/accept")]
        [SwaggerOperation(OperationId = "AcceptCertificateRequest")]
        [Authorize(Policy = Policies.CanWrite)]
        public async Task AcceptCertificateRequestAsync(string requestId)
        {
            await _certificateRequest.AcceptAsync(requestId);
        }

        /// <summary>Query certificate requests</summary>
        [HttpGet]
        [SwaggerOperation(OperationId = "QueryRequests")]
        public async Task<CertificateRequestRecordQueryResponseApiModel> QueryRequestsAsync()
        {
            var results = await _certificateRequest.QueryAsync(null, null);
            return new CertificateRequestRecordQueryResponseApiModel(results);
        }

        /// <summary>Query certificate requests by appId</summary>
        [HttpGet("app/{appId}")]
        [SwaggerOperation(OperationId = "QueryAppRequests")]
        public async Task<CertificateRequestRecordQueryResponseApiModel> QueryAppRequestsAsync(string appId)
        {
            var results = await _certificateRequest.QueryAsync(appId, null);
            return new CertificateRequestRecordQueryResponseApiModel(results);
        }

        /// <summary>Query certificate requests by state</summary>
        [HttpGet("state/{state}")]
        [SwaggerOperation(OperationId = "QueryAppRequests")]
        public async Task<CertificateRequestRecordQueryResponseApiModel> QueryStateRequestsAsync(string state)
        {
            Contract.Requires(string.IsNullOrEmpty(state) == false);
            // todo: parse state
            var results = await _certificateRequest.QueryAsync(null, null);
            return new CertificateRequestRecordQueryResponseApiModel(results);
        }

        /// <summary>Read certificate request</summary>
        [HttpGet("{requestId}")]
        [SwaggerOperation(OperationId = "ReadCertificateRequest")]
        public async Task<CertificateRequestRecordApiModel> ReadCertificateRequestAsync(string requestId)
        {
            var result = await _certificateRequest.ReadAsync(requestId);
            return new CertificateRequestRecordApiModel(
                requestId,
                result.ApplicationId,
                result.State,
                result.CertificateGroupId,
                result.CertificateTypeId,
                result.SigningRequest,
                result.SubjectName,
                result.DomainNames,
                result.PrivateKeyFormat);
        }

        /// <summary>Complete certificate request</summary>
        [HttpPost("{requestId}/{applicationId}/finish")]
        [SwaggerOperation(OperationId = "FinishRequest")]
        [Authorize(Policy = Policies.CanWrite)]
        public async Task<FinishRequestApiModel> FinishRequestAsync(string requestId, string applicationId)
        {
            var result = await _certificateRequest.FinishRequestAsync(
                requestId,
                applicationId
                );
            return new FinishRequestApiModel(
                requestId,
                applicationId,
                result.State,
                result.CertificateGroupId,
                result.CertificateTypeId,
                result.SignedCertificate,
                result.PrivateKeyFormat,
                result.PrivateKey,
                result.AuthorityId
                );
        }

    }
}