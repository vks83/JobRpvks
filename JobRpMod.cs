using System;
using System.Collections.Generic;
using MelonLoader;
using BoneLib;
using BoneLib.BoneMenu;
using BoneLib.BoneMenu.Elements;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Extensions;
using UnityEngine;

// Liaison stricte avec les métadonnées de votre projet GitHub JobRpvks
[assembly: MelonInfo(typeof(JobRpvks.JobRpMod), "JobRpvks", "1.0.0", "vks83")]
[assembly: MelonGame("Stress Level Zero", "BONELAB")]

namespace JobRpvks
{
    public class JobRpMod : MelonMod
    {
        // Structure pour coupler les métiers et grades de style DarkRP
        public class JobConfig
        {
            public string JobName;
            public Color JobColor;
            public string[] Grades;
        }

        private static Dictionary<string, JobConfig> jobsDatabase = new Dictionary<string, JobConfig>();
        private static MenuCategory jobRpMenu;

        public override void OnInitializeMelon()
        {
            SetupJobs();
            HookBoneLib();
        }

        private void SetupJobs()
        {
            // Configuration de l'équipe de Police (Style DarkRP)
            jobsDatabase.Add("Police", new JobConfig
            {
                JobName = "Police",
                JobColor = Color.cyan, // Bleu clair pour la justice
                Grades = new string[] { "Cadet", "Officier", "Sergent", "Commissaire" }
            });

            // Configuration de l'équipe des Gangsters
            jobsDatabase.Add("Gangster", new JobConfig
            {
                JobName = "Gangster",
                JobColor = Color.red, // Rouge pour le crime
                Grades = new string[] { "Recrue", "Braqueur", "Lieutenant", "Parrain" }
            });
        }

        private void HookBoneLib()
        {
            // On greffe l'initialisation du menu VR au chargement des niveaux
            BoneLib.Hooking.OnLevelLoaded += (info) =>
            {
                if (jobRpMenu == null)
                {
                    CreateJobRpMenu();
                }
            };
        }

        private void CreateJobRpMenu()
        {
            // Création de votre menu principal cliquable "Job RP VKS"
            jobRpMenu = MenuManager.CreateCategory("Job RP VKS", Color.yellow);
            var selectJobCategory = jobRpMenu.CreateCategory("Choisir mon Métier", Color.white);

            foreach (var jobPair in jobsDatabase)
            {
                var job = jobPair.Value;
                var jobSubMenu = selectJobCategory.CreateCategory(job.JobName, job.JobColor);

                // Génération automatique des boutons de grade à la manette Quest
                foreach (var grade in job.Grades)
                {
                    string tagComplet = $"[{job.JobName} - {grade}]";
                    
                    jobSubMenu.CreateFunctionElement($"{grade}", job.JobColor, () =>
                    {
                        ApplyJobTagToSelf(tagComplet, job.JobColor);
                    });
                }
            }

            // Option pour retirer son rôle instantanément
            jobRpMenu.CreateFunctionElement("Démissionner (Quitter Job)", Color.gray, () =>
            {
                RemoveJobTagFromSelf();
            });
        }

        private void ApplyJobTagToSelf(string tag, Color color)
        {
            try
            {
                // Vérification de la présence d'un serveur actif LabFusion
                if (NetworkInfo.HasServer)
                {
                    // Injection de la couleur en Rich Text pour le Quest 3
                    string hexColor = ColorUtility.ToHtmlStringRGB(color);
                    string finalNickname = $"<color=#{hexColor}>{tag}</color> {PlayerIdManager.LocalUsername}";

                    // Synchronisation réseau via LabFusion
                    PlayerIdManager.LocalId.InsertMetadata("Fusion_Nickname", finalNickname);
                    MelonLogger.Msg($"Métier appliqué avec succès : {tag}");
                }
                else
                {
                    MelonLogger.Warning("Tu dois être dans un serveur LabFusion pour appliquer un rôle !");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Erreur lors de l'application du Tag: {ex.Message}");
            }
        }

        private void RemoveJobTagFromSelf()
        {
            if (NetworkInfo.HasServer)
            {
                // Restauration du pseudo initial de l'utilisateur
                PlayerIdManager.LocalId.InsertMetadata("Fusion_Nickname", PlayerIdManager.LocalUsername);
                MelonLogger.Msg("Vous avez démissionné de votre métier.");
            }
        }
    }
}
