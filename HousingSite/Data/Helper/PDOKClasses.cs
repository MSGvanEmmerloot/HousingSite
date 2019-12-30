using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HousingSite.Data.Helper
{
    // Class to retrieve all data
    public class PDOKRootObject
    {
        public PDOKResponse response { get; set; }
    }
    public class PDOKResponse
    {
        public int numFound { get; set; }
        public int start { get; set; }
        public double maxScore { get; set; }
        public List<PDOKDoc> docs { get; set; }
    }
    public class PDOKDoc
    {
        public string bron { get; set; }
        public string woonplaatscode { get; set; }
        public string type { get; set; }
        public string woonplaatsnaam { get; set; }
        public string openbareruimtetype { get; set; }
        public string gemeentecode { get; set; }
        public string weergavenaam { get; set; }
        public string straatnaam_verkort { get; set; }
        public string id { get; set; }
        public string gemeentenaam { get; set; }
        public string identificatie { get; set; }
        public string openbareruimte_id { get; set; }
        public string provinciecode { get; set; }
        public string postcode { get; set; }
        public string provincienaam { get; set; }
        public string centroide_ll { get; set; }
        public string provincieafkorting { get; set; }
        public string centroide_rd { get; set; }
        public string straatnaam { get; set; }
        public double score { get; set; }
    }    

    // Class to retrieve coordinates from address
    public class PDOKRootObjectCoords
    {
        public PDOKResponseCoords response { get; set; }
    }
    public class PDOKResponseCoords
    {
        public int numFound { get; set; }
        public int start { get; set; }
        public double maxScore { get; set; }
        public List<PDOKDocCoords> docs { get; set; }
    }
    public class PDOKDocCoords
    {
        public string geometrie_ll { get; set; }
    }    

    // Class to retrieve address from coordinates
    public class PDOKRootObjectAddress
    {
        public PDOKResponseAddress response { get; set; }
    }
    public class PDOKResponseAddress
    {
        public int numFound { get; set; }
        public int start { get; set; }
        public double maxScore { get; set; }
        public List<PDOKDocAddress> docs { get; set; }
    }
    public class PDOKDocAddress
    {
        public string weergavenaam { get; set; }
    }
}
