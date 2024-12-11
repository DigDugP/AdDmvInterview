using Alvin.Shared.ApiLibrary.Database;
using Alvin.Shared.ApiLibrary.Generators.Receipt;
using Alvin.Shared.ApiLibrary.Models;
using Alvin.Shared.CommonLibrary.Codes;
using Alvin.Shared.CommonLibrary.Generators;

namespace Alvin.Shared.ApiLibrary.Generators.RegistrationCreate
{
    public interface IRegistrationTabNumberGenerator
    {
        string Execute(string propertyClassCode, int? expirationYear, string licensePlateNumber, string licensePlateTypeCode, string propertyTaxExemptTypeCode);
    }

    public class RegistrationTabNumberGenerator : BaseGenerator, IRegistrationTabNumberGenerator
    {
        public IAlvinContext alvinContext { get; set; }
        public IPermanentRegistrationGenerator _permanentRegistrationGenerator;

        public RegistrationTabNumberGenerator(IAlvinContext alvinContext, IPermanentRegistrationGenerator permanentRegistrationGenerator)
        {
            this.alvinContext = alvinContext;
            _permanentRegistrationGenerator = permanentRegistrationGenerator;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyClassCode"></param>
        /// <param name="expirationYear"></param>
        /// <param name="licensePlateNumber"></param>
        /// <param name="licensePlateTypeCode"></param>
        /// <param name="propertyTaxExemptTypeCode"></param>
        /// <returns></returns>
        public string Execute(string propertyClassCode, int? expirationYear, string licensePlateNumber, string licensePlateTypeCode, string propertyTaxExemptTypeCode)
        {
            string tabPrefix = GetTabPrefix(propertyClassCode, expirationYear, licensePlateNumber, licensePlateTypeCode, propertyTaxExemptTypeCode);
            if (tabPrefix == null)
            {
                return null;
            }

            return tabPrefix + licensePlateNumber;
        }

        public string GetTabPrefix(string propertyClassCode, int? expirationYear, string licensePlateNumber, string licensePlateTypeCode, string propertyTaxExemptTypeCode)
        {
            IAlvinReferenceContext alvinReference;
            RegistrationTabPrefixType tabPrefix;
            string code;

            if (_permanentRegistrationGenerator.IsHamRadio(propertyClassCode))
            {
                alvinReference = alvinContext.Reference;
                tabPrefix = alvinReference.RegistrationTabPrefixTypeByYear(expirationYear);
                code = tabPrefix.Code;
                return code;
            }

            if (_permanentRegistrationGenerator.IsHistoricExhibitionVehicle(propertyClassCode, licensePlateNumber))
            {
                return null;
            }

            // check to see if this is a commercial trailer
            if (_permanentRegistrationGenerator.IsCommercialTrailer(propertyClassCode))
            {
                alvinReference = alvinContext.Reference;
                tabPrefix = alvinReference.RegistrationTabPrefixTypeByYear(expirationYear);
                code = tabPrefix.Code;
                return code;
            }

            if (_permanentRegistrationGenerator.IsStateVehicle(propertyClassCode, licensePlateNumber))
            {
                return null;
            }

            if (propertyTaxExemptTypeCode == PropertyTaxExemptTypeCodes.Z_PermRegFor8YearAndOlder)
            {
                return PropertyTaxExemptTypeCodes.Z_PermRegFor8YearAndOlder;
            }

            string x = "P000000";
            if (_permanentRegistrationGenerator.Execute(propertyClassCode, licensePlateNumber, licensePlateTypeCode, propertyTaxExemptTypeCode, x))
            {
                return RegistrationTabPrefixTypeCodes.P_PTab;
            }

            alvinReference = alvinContext.Reference;
            tabPrefix = alvinReference.RegistrationTabPrefixTypeByYear(expirationYear);
            code = tabPrefix.Code;
            return code;
        }
    }
}
